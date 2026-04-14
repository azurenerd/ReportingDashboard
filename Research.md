# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-14 01:09 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and monthly heatmaps. The dashboard reads from a local `data.json` file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint decks. Given the simplicity requirements (no auth, no cloud, no enterprise security), Blazor Server is well-suited: it provides full C# rendering, real-time DOM updates, and zero JavaScript framework overhead. The primary technical challenge is faithfully reproducing the SVG timeline and CSS Grid heatmap from the reference HTML design. We recommend a minimal architecture: a single Blazor Server project with inline CSS, a JSON data model, and no database—just a flat `data.json` file on disk. ---

### Key Findings

- The reference design uses **CSS Grid** (`160px repeat(4,1fr)`) for the heatmap and **SVG** for the timeline—both render natively in Blazor without any JavaScript libraries.
- Blazor Server in .NET 8 supports **Static SSR (Server-Side Rendering)** which is ideal for a read-only dashboard that will be screenshotted.
- No charting library is necessary; the SVG timeline is simple enough (lines, circles, diamonds, text) to render directly in Razor markup.
- `System.Text.Json` (built into .NET 8) handles all JSON deserialization needs—no third-party packages required.
- The entire solution can be a **single `.csproj`** with fewer than 10 files total.
- The color palette, fonts, and layout from `OriginalDesignConcept.html` can be ported directly into a Blazor component's `<style>` block or a single CSS file.
- For screenshot fidelity, the page should be fixed at **1920×1080** with `overflow:hidden`, matching the original design.
- No database is needed—`data.json` is read on each page load (or cached in memory via a singleton service).
- The design's four status rows (Shipped, In Progress, Carryover, Blockers) map cleanly to a typed C# model with enum-based status categories.
- Hot reload in .NET 8 Blazor Server allows rapid iteration on layout and styling during development. ---
- Create the .NET 8 Blazor Server project (`dotnet new blazor -n ReportingDashboard --interactivity Server`)
- Port `OriginalDesignConcept.html` CSS and markup directly into a single `Dashboard.razor` component
- Hardcode all data inline to match the reference exactly
- **Goal:** Pixel-perfect match of the reference design running in Blazor
- Define the C# data model (`DashboardData`, `Milestone`, `StatusRow`)
- Create `data.json` with fictional project data
- Build `DashboardDataService` to read and deserialize the JSON
- Replace hardcoded values with `@foreach` loops and data bindings
- **Goal:** Change `data.json` → refresh browser → see updated dashboard
- Dynamic SVG timeline positioning (date-to-pixel mapping)
- Current month auto-highlighting based on `DateTime.Now` or `data.json` config
- CSS custom properties for easy color theming
- Error handling for malformed JSON (display friendly error message)
- Create `data.template.json` with schema documentation
- **Goal:** Production-quality dashboard ready for executive screenshots
- **Start with the CSS port.** The reference HTML is the spec—copy its `<style>` block verbatim and adapt for Blazor's CSS isolation. This gives immediate visual validation.
- **Use `dotnet watch`** for instant hot reload while tweaking CSS and layout.
- **Create a realistic `data.json` example** with a fictional "Project Phoenix" or similar—this makes demos and screenshots immediately compelling.
```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Platform Engineering • Core Infrastructure • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "currentDate": "2026-04-14",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "tracks": [
    {
      "id": "M1",
      "label": "API Gateway & Auth",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-15", "type": "Checkpoint", "label": "Design Review" },
        { "date": "2026-03-20", "type": "PoC", "label": "Mar 20 PoC" },
        { "date": "2026-05-01", "type": "Production", "label": "May Prod" }
      ]
    }
  ],
  "statusRows": {
    "shipped": {
      "Jan": ["API design doc", "Auth provider selection"],
      "Feb": ["JWT middleware", "Rate limiting v1"],
      "Mar": ["Gateway routing", "Health checks"],
      "Apr": ["Load testing complete"]
    },
    "inProgress": {
      "Jan": [],
      "Feb": ["OAuth2 flows"],
      "Mar": ["Dashboard UI"],
      "Apr": ["E2E integration tests", "Canary deployment"]
    },
    "carryover": {
      "Jan": [],
      "Feb": [],
      "Mar": ["Logging pipeline"],
      "Apr": ["Metrics aggregation"]
    },
    "blockers": {
      "Jan": [],
      "Feb": [],
      "Mar": [],
      "Apr": ["Vendor SDK delay", "Perf regression in staging"]
    }
  }
}
```
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── data.json
│   ├── data.template.json
│   ├── Components/
│   │   ├── App.razor
│   │   ├── Routes.razor
│   │   ├── Layout/
│   │   │   └── MainLayout.razor
│   │   └── Pages/
│   │       ├── Dashboard.razor
│   │       ├── Dashboard.razor.css
│   │       └── _Imports.razor
│   ├── Models/
│   │   └── DashboardData.cs
│   ├── Services/
│   │   └── DashboardDataService.cs
│   └── wwwroot/
│       └── app.css
└── README.md
``` **None beyond what the template provides.** The default `blazor` template includes everything needed:
- `Microsoft.AspNetCore.Components.Web` (Blazor rendering)
- `System.Text.Json` (JSON deserialization)
- `Microsoft.Extensions.FileProviders.Physical` (file watching, built-in) This is a zero-dependency project beyond the .NET 8 SDK itself.

### Recommended Tools & Technologies

- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Use `@rendermode InteractiveServer` or static SSR | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Port directly from reference HTML; no CSS framework needed | | **SVG Timeline** | Inline SVG in Razor | N/A | Render `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` directly in `.razor` files | | **Charting Library** | **None required** | — | The timeline is simple geometry; a charting lib would add unnecessary complexity | | **Icons** | Inline SVG or Unicode | — | Diamond shapes (◆) and circles (●) are rendered via SVG primitives | | **CSS Isolation** | Blazor CSS isolation (`.razor.css`) | Built-in | One CSS file per component, scoped automatically | | **Font** | Segoe UI (system font) | — | Already available on Windows; fallback to Arial | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility | | **Data File** | `data.json` in project root or `wwwroot/data/` | — | Read via `IWebHostEnvironment.ContentRootPath` or `WebRootPath` | | **Data Caching** | Singleton `IMemoryCache` or simple singleton service | `Microsoft.Extensions.Caching.Memory` (built-in) | Cache parsed JSON; invalidate on file change via `IFileProvider.Watch()` | | **Database** | **None** | — | A flat JSON file is the correct choice for this scope | | Component | Recommendation | Notes | |-----------|---------------|-------| | **Solution** | Single `.sln` with one `.csproj` | `ReportingDashboard.sln` → `ReportingDashboard.csproj` | | **Project Template** | `blazorserver` (or `blazor` with server rendermode) | `dotnet new blazor --interactivity Server` | | **Target Framework** | `net8.0` | LTS until November 2026 | | Tool | Version | Purpose | |------|---------|---------| | **.NET SDK** | 8.0.x (latest patch) | Build and run | | **Visual Studio 2022** or **VS Code + C# Dev Kit** | 17.8+ / latest | IDE with hot reload | | **dotnet watch** | Built-in | Hot reload during CSS/layout iteration | | **Browser DevTools** | Any Chromium browser | Inspect CSS Grid layout, validate 1920×1080 rendering | | Tool | Version | Purpose | |------|---------|---------| | **bUnit** | 1.25+ | Blazor component unit tests (optional given scope) | | **xUnit** | 2.7+ | Test runner | | **Verify** | 25.x | Snapshot testing for rendered HTML output (optional) | --- Given the extreme simplicity requirement, avoid over-engineering. The architecture is:
```
data.json → DashboardDataService (singleton) → Dashboard.razor (single page) → Browser
``` Map the visual design to **3-4 Blazor components**:
```
App.razor
└── Dashboard.razor              (main page, full 1920×1080 layout)
    ├── DashboardHeader.razor    (title, subtitle, legend)
    ├── TimelineSvg.razor        (SVG milestone visualization)
    └── HeatmapGrid.razor        (CSS Grid status matrix)
        ├── HeatmapRowHeader      (Shipped/InProgress/Carryover/Blockers labels)
        └── HeatmapCell           (individual month×status cell with item bullets)
```
```csharp
public record DashboardData
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string CurrentMonth { get; init; }
    public List<string> Months { get; init; }          // ["Jan","Feb","Mar","Apr"]
    public List<Milestone> Milestones { get; init; }
    public List<StatusRow> StatusRows { get; init; }    // Shipped, InProgress, Carryover, Blockers
}

public record Milestone
{
    public string Label { get; init; }
    public string Date { get; init; }
    public MilestoneType Type { get; init; }            // PoC, Production, Checkpoint
    public string TrackId { get; init; }                // M1, M2, M3
}

public record StatusRow
{
    public StatusCategory Category { get; init; }       // Shipped, InProgress, Carryover, Blockers
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
}

public enum StatusCategory { Shipped, InProgress, Carryover, Blockers }
public enum MilestoneType { Checkpoint, PoC, Production }
```
- **Startup:** `DashboardDataService` registered as singleton; reads and deserializes `data.json` once.
- **File watching (optional):** Use `PhysicalFileProvider.Watch("data.json")` to auto-reload when the file changes—useful during development.
- **Render:** `Dashboard.razor` injects `DashboardDataService`, passes data to child components via `[Parameter]`.
- **No interactivity needed:** The page is read-only. Static SSR (`@rendermode` omitted or set to `null`) is sufficient and eliminates the WebSocket connection overhead.
- **Port the reference CSS directly.** The `OriginalDesignConcept.html` styles are clean and well-structured. Copy them into `Dashboard.razor.css` or a global `app.css`.
- Use **CSS custom properties** (variables) for the color palette to make theming easy:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-current-month-highlight: /* slightly darker variant per row */;
}
```
- Keep the **fixed 1920×1080 layout** (`body { width: 1920px; height: 1080px; overflow: hidden; }`). This is designed for screenshots, not responsive use.
- The CSS Grid for the heatmap (`grid-template-columns: 160px repeat(N, 1fr)`) should be dynamic based on the number of months in `data.json`. Use inline `style` binding in Razor: `style="grid-template-columns: 160px repeat(@months.Count, 1fr)"`. The timeline SVG should be generated dynamically from `data.json`:
- Calculate X positions based on date ranges (map date → pixel position within the SVG width).
- Render track lines (`<line>`), milestone diamonds (`<polygon>`), checkpoints (`<circle>`), and labels (`<text>`).
- The "NOW" indicator is a dashed red vertical line at the current date's X position.
- Use Blazor's `@foreach` to iterate over milestones and render SVG elements. ---

### Considerations & Risks

- **None.** Per requirements, no auth is needed. The dashboard runs locally and is accessed via `https://localhost:5001` or `http://localhost:5000`.
- If sharing on a team network becomes needed later, consider adding a simple API key middleware or Windows Authentication (one line in `Program.cs`).
- `data.json` is a local file. No encryption needed for this use case.
- If the JSON contains sensitive project names, ensure the file is in `.gitignore` and provide a `data.template.json` with example/fake data for the repo. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local Kestrel (built into .NET 8) | | **Launch** | `dotnet run` from command line, or F5 in Visual Studio | | **Port** | Default `https://localhost:5001` | | **Deployment** | Not applicable—runs on developer's machine | | **Containerization** | Not needed for local-only use | Since the primary output is PowerPoint screenshots:
- Open `http://localhost:5000` in a Chromium browser.
- Set browser window to 1920×1080 (use DevTools device toolbar or a browser extension like "Window Resizer").
- Take screenshot (or use `Ctrl+Shift+P` → "Capture full size screenshot" in DevTools).
- Alternatively, add a `/screenshot` endpoint later using Playwright if automation is desired.
- **$0.** Everything runs locally. No cloud services, no hosting costs. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | SVG timeline positioning math is tricky | Medium | Low | Start with hardcoded positions matching the reference, then make dynamic | | CSS Grid differences between browsers | Low | Low | Target Chromium only (screenshot tool); no cross-browser concerns | | `data.json` schema changes break rendering | Medium | Low | Use nullable properties and null-conditional rendering in Razor | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | No database | Can't query historical data | Simplicity is the goal; JSON file is sufficient for a single dashboard | | No charting library | Must hand-code SVG | The timeline is simple enough; a charting lib (Chart.js, Plotly) would add 200KB+ of JS and configuration overhead for minimal benefit | | Fixed 1920×1080 layout | Not responsive on mobile/tablet | Explicitly designed for screenshot capture; responsiveness adds complexity with no value | | Static SSR (no WebSocket) | No real-time updates | Dashboard is refreshed by page reload; real-time updates unnecessary for screenshot workflows | | No CSS framework (Bootstrap, Tailwind) | Must write all CSS manually | The reference HTML already has all needed CSS; a framework would fight the custom grid layout |
- **Over-engineering:** The biggest risk is making this more complex than it needs to be. Resist adding features beyond the reference design.
- **SVG coordinate math:** Mapping dates to pixel positions requires a simple linear interpolation function. Test with edge cases (milestones at month boundaries).
- **JSON schema drift:** If someone edits `data.json` incorrectly, the page will break silently. Add a try-catch in the data service with a clear error message rendered on the page. --- | # | Question | Who Decides | Impact | |---|----------|-------------|--------| | 1 | Should the number of months be configurable (e.g., 4 months, 6 months, 12 months)? | Product Owner | Affects CSS Grid column count and SVG width calculations | | 2 | Should `data.json` live in `wwwroot/` (accessible via URL) or in the project root (server-only)? | Developer | Security vs. convenience trade-off | | 3 | Is the "ADO Backlog" link in the header functional, or just visual for the screenshot? | Product Owner | Determines if we need external URL support in `data.json` | | 4 | How many milestone tracks (M1, M2, M3…) should the timeline support? | Product Owner | Affects SVG height and track spacing calculations | | 5 | Should there be a print/export-to-PDF feature, or is manual screenshotting sufficient? | Product Owner | Could add Playwright-based PDF export later | | 6 | Will multiple projects each have their own `data.json`, or is this always a single-project dashboard? | Product Owner | Affects routing and file organization | | 7 | Should the "current month" column highlight be automatic (based on system date) or specified in `data.json`? | Developer | Auto-detection is easy but `data.json` control is more flexible for preparing future decks | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and monthly heatmaps. The dashboard reads from a local `data.json` file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint decks. Given the simplicity requirements (no auth, no cloud, no enterprise security), Blazor Server is well-suited: it provides full C# rendering, real-time DOM updates, and zero JavaScript framework overhead. The primary technical challenge is faithfully reproducing the SVG timeline and CSS Grid heatmap from the reference HTML design. We recommend a minimal architecture: a single Blazor Server project with inline CSS, a JSON data model, and no database—just a flat `data.json` file on disk.

---

## 2. Key Findings

- The reference design uses **CSS Grid** (`160px repeat(4,1fr)`) for the heatmap and **SVG** for the timeline—both render natively in Blazor without any JavaScript libraries.
- Blazor Server in .NET 8 supports **Static SSR (Server-Side Rendering)** which is ideal for a read-only dashboard that will be screenshotted.
- No charting library is necessary; the SVG timeline is simple enough (lines, circles, diamonds, text) to render directly in Razor markup.
- `System.Text.Json` (built into .NET 8) handles all JSON deserialization needs—no third-party packages required.
- The entire solution can be a **single `.csproj`** with fewer than 10 files total.
- The color palette, fonts, and layout from `OriginalDesignConcept.html` can be ported directly into a Blazor component's `<style>` block or a single CSS file.
- For screenshot fidelity, the page should be fixed at **1920×1080** with `overflow:hidden`, matching the original design.
- No database is needed—`data.json` is read on each page load (or cached in memory via a singleton service).
- The design's four status rows (Shipped, In Progress, Carryover, Blockers) map cleanly to a typed C# model with enum-based status categories.
- Hot reload in .NET 8 Blazor Server allows rapid iteration on layout and styling during development.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Use `@rendermode InteractiveServer` or static SSR |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Port directly from reference HTML; no CSS framework needed |
| **SVG Timeline** | Inline SVG in Razor | N/A | Render `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` directly in `.razor` files |
| **Charting Library** | **None required** | — | The timeline is simple geometry; a charting lib would add unnecessary complexity |
| **Icons** | Inline SVG or Unicode | — | Diamond shapes (◆) and circles (●) are rendered via SVG primitives |
| **CSS Isolation** | Blazor CSS isolation (`.razor.css`) | Built-in | One CSS file per component, scoped automatically |
| **Font** | Segoe UI (system font) | — | Already available on Windows; fallback to Arial |

### Backend / Data Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility |
| **Data File** | `data.json` in project root or `wwwroot/data/` | — | Read via `IWebHostEnvironment.ContentRootPath` or `WebRootPath` |
| **Data Caching** | Singleton `IMemoryCache` or simple singleton service | `Microsoft.Extensions.Caching.Memory` (built-in) | Cache parsed JSON; invalidate on file change via `IFileProvider.Watch()` |
| **Database** | **None** | — | A flat JSON file is the correct choice for this scope |

### Project Structure

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Solution** | Single `.sln` with one `.csproj` | `ReportingDashboard.sln` → `ReportingDashboard.csproj` |
| **Project Template** | `blazorserver` (or `blazor` with server rendermode) | `dotnet new blazor --interactivity Server` |
| **Target Framework** | `net8.0` | LTS until November 2026 |

### Development Tools

| Tool | Version | Purpose |
|------|---------|---------|
| **.NET SDK** | 8.0.x (latest patch) | Build and run |
| **Visual Studio 2022** or **VS Code + C# Dev Kit** | 17.8+ / latest | IDE with hot reload |
| **dotnet watch** | Built-in | Hot reload during CSS/layout iteration |
| **Browser DevTools** | Any Chromium browser | Inspect CSS Grid layout, validate 1920×1080 rendering |

### Testing (Minimal)

| Tool | Version | Purpose |
|------|---------|---------|
| **bUnit** | 1.25+ | Blazor component unit tests (optional given scope) |
| **xUnit** | 2.7+ | Test runner |
| **Verify** | 25.x | Snapshot testing for rendered HTML output (optional) |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Component Page with Data Service**

Given the extreme simplicity requirement, avoid over-engineering. The architecture is:

```
data.json → DashboardDataService (singleton) → Dashboard.razor (single page) → Browser
```

### Component Architecture

Map the visual design to **3-4 Blazor components**:

```
App.razor
└── Dashboard.razor              (main page, full 1920×1080 layout)
    ├── DashboardHeader.razor    (title, subtitle, legend)
    ├── TimelineSvg.razor        (SVG milestone visualization)
    └── HeatmapGrid.razor        (CSS Grid status matrix)
        ├── HeatmapRowHeader      (Shipped/InProgress/Carryover/Blockers labels)
        └── HeatmapCell           (individual month×status cell with item bullets)
```

### Data Model (C#)

```csharp
public record DashboardData
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string CurrentMonth { get; init; }
    public List<string> Months { get; init; }          // ["Jan","Feb","Mar","Apr"]
    public List<Milestone> Milestones { get; init; }
    public List<StatusRow> StatusRows { get; init; }    // Shipped, InProgress, Carryover, Blockers
}

public record Milestone
{
    public string Label { get; init; }
    public string Date { get; init; }
    public MilestoneType Type { get; init; }            // PoC, Production, Checkpoint
    public string TrackId { get; init; }                // M1, M2, M3
}

public record StatusRow
{
    public StatusCategory Category { get; init; }       // Shipped, InProgress, Carryover, Blockers
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
}

public enum StatusCategory { Shipped, InProgress, Carryover, Blockers }
public enum MilestoneType { Checkpoint, PoC, Production }
```

### Data Flow

1. **Startup:** `DashboardDataService` registered as singleton; reads and deserializes `data.json` once.
2. **File watching (optional):** Use `PhysicalFileProvider.Watch("data.json")` to auto-reload when the file changes—useful during development.
3. **Render:** `Dashboard.razor` injects `DashboardDataService`, passes data to child components via `[Parameter]`.
4. **No interactivity needed:** The page is read-only. Static SSR (`@rendermode` omitted or set to `null`) is sufficient and eliminates the WebSocket connection overhead.

### CSS Strategy

- **Port the reference CSS directly.** The `OriginalDesignConcept.html` styles are clean and well-structured. Copy them into `Dashboard.razor.css` or a global `app.css`.
- Use **CSS custom properties** (variables) for the color palette to make theming easy:

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-current-month-highlight: /* slightly darker variant per row */;
}
```

- Keep the **fixed 1920×1080 layout** (`body { width: 1920px; height: 1080px; overflow: hidden; }`). This is designed for screenshots, not responsive use.
- The CSS Grid for the heatmap (`grid-template-columns: 160px repeat(N, 1fr)`) should be dynamic based on the number of months in `data.json`. Use inline `style` binding in Razor: `style="grid-template-columns: 160px repeat(@months.Count, 1fr)"`.

### SVG Timeline Rendering

The timeline SVG should be generated dynamically from `data.json`:

- Calculate X positions based on date ranges (map date → pixel position within the SVG width).
- Render track lines (`<line>`), milestone diamonds (`<polygon>`), checkpoints (`<circle>`), and labels (`<text>`).
- The "NOW" indicator is a dashed red vertical line at the current date's X position.
- Use Blazor's `@foreach` to iterate over milestones and render SVG elements.

---

## 5. Security & Infrastructure

### Authentication & Authorization

- **None.** Per requirements, no auth is needed. The dashboard runs locally and is accessed via `https://localhost:5001` or `http://localhost:5000`.
- If sharing on a team network becomes needed later, consider adding a simple API key middleware or Windows Authentication (one line in `Program.cs`).

### Data Protection

- `data.json` is a local file. No encryption needed for this use case.
- If the JSON contains sensitive project names, ensure the file is in `.gitignore` and provide a `data.template.json` with example/fake data for the repo.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local Kestrel (built into .NET 8) |
| **Launch** | `dotnet run` from command line, or F5 in Visual Studio |
| **Port** | Default `https://localhost:5001` |
| **Deployment** | Not applicable—runs on developer's machine |
| **Containerization** | Not needed for local-only use |

### Screenshot Workflow

Since the primary output is PowerPoint screenshots:

1. Open `http://localhost:5000` in a Chromium browser.
2. Set browser window to 1920×1080 (use DevTools device toolbar or a browser extension like "Window Resizer").
3. Take screenshot (or use `Ctrl+Shift+P` → "Capture full size screenshot" in DevTools).
4. Alternatively, add a `/screenshot` endpoint later using Playwright if automation is desired.

### Infrastructure Costs

- **$0.** Everything runs locally. No cloud services, no hosting costs.

---

## 6. Risks & Trade-offs

### Low-Risk Items

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| SVG timeline positioning math is tricky | Medium | Low | Start with hardcoded positions matching the reference, then make dynamic |
| CSS Grid differences between browsers | Low | Low | Target Chromium only (screenshot tool); no cross-browser concerns |
| `data.json` schema changes break rendering | Medium | Low | Use nullable properties and null-conditional rendering in Razor |

### Trade-offs Made

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No database | Can't query historical data | Simplicity is the goal; JSON file is sufficient for a single dashboard |
| No charting library | Must hand-code SVG | The timeline is simple enough; a charting lib (Chart.js, Plotly) would add 200KB+ of JS and configuration overhead for minimal benefit |
| Fixed 1920×1080 layout | Not responsive on mobile/tablet | Explicitly designed for screenshot capture; responsiveness adds complexity with no value |
| Static SSR (no WebSocket) | No real-time updates | Dashboard is refreshed by page reload; real-time updates unnecessary for screenshot workflows |
| No CSS framework (Bootstrap, Tailwind) | Must write all CSS manually | The reference HTML already has all needed CSS; a framework would fight the custom grid layout |

### Potential Pitfalls

1. **Over-engineering:** The biggest risk is making this more complex than it needs to be. Resist adding features beyond the reference design.
2. **SVG coordinate math:** Mapping dates to pixel positions requires a simple linear interpolation function. Test with edge cases (milestones at month boundaries).
3. **JSON schema drift:** If someone edits `data.json` incorrectly, the page will break silently. Add a try-catch in the data service with a clear error message rendered on the page.

---

## 7. Open Questions

| # | Question | Who Decides | Impact |
|---|----------|-------------|--------|
| 1 | Should the number of months be configurable (e.g., 4 months, 6 months, 12 months)? | Product Owner | Affects CSS Grid column count and SVG width calculations |
| 2 | Should `data.json` live in `wwwroot/` (accessible via URL) or in the project root (server-only)? | Developer | Security vs. convenience trade-off |
| 3 | Is the "ADO Backlog" link in the header functional, or just visual for the screenshot? | Product Owner | Determines if we need external URL support in `data.json` |
| 4 | How many milestone tracks (M1, M2, M3…) should the timeline support? | Product Owner | Affects SVG height and track spacing calculations |
| 5 | Should there be a print/export-to-PDF feature, or is manual screenshotting sufficient? | Product Owner | Could add Playwright-based PDF export later |
| 6 | Will multiple projects each have their own `data.json`, or is this always a single-project dashboard? | Product Owner | Affects routing and file organization |
| 7 | Should the "current month" column highlight be automatic (based on system date) or specified in `data.json`? | Developer | Auto-detection is easy but `data.json` control is more flexible for preparing future decks |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Static Port (1-2 hours)
- Create the .NET 8 Blazor Server project (`dotnet new blazor -n ReportingDashboard --interactivity Server`)
- Port `OriginalDesignConcept.html` CSS and markup directly into a single `Dashboard.razor` component
- Hardcode all data inline to match the reference exactly
- **Goal:** Pixel-perfect match of the reference design running in Blazor

#### Phase 2: Data-Driven Rendering (1-2 hours)
- Define the C# data model (`DashboardData`, `Milestone`, `StatusRow`)
- Create `data.json` with fictional project data
- Build `DashboardDataService` to read and deserialize the JSON
- Replace hardcoded values with `@foreach` loops and data bindings
- **Goal:** Change `data.json` → refresh browser → see updated dashboard

#### Phase 3: Polish & Improvements (1 hour)
- Dynamic SVG timeline positioning (date-to-pixel mapping)
- Current month auto-highlighting based on `DateTime.Now` or `data.json` config
- CSS custom properties for easy color theming
- Error handling for malformed JSON (display friendly error message)
- Create `data.template.json` with schema documentation
- **Goal:** Production-quality dashboard ready for executive screenshots

### Quick Wins

1. **Start with the CSS port.** The reference HTML is the spec—copy its `<style>` block verbatim and adapt for Blazor's CSS isolation. This gives immediate visual validation.
2. **Use `dotnet watch`** for instant hot reload while tweaking CSS and layout.
3. **Create a realistic `data.json` example** with a fictional "Project Phoenix" or similar—this makes demos and screenshots immediately compelling.

### Recommended `data.json` Structure

```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Platform Engineering • Core Infrastructure • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "currentDate": "2026-04-14",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "tracks": [
    {
      "id": "M1",
      "label": "API Gateway & Auth",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-15", "type": "Checkpoint", "label": "Design Review" },
        { "date": "2026-03-20", "type": "PoC", "label": "Mar 20 PoC" },
        { "date": "2026-05-01", "type": "Production", "label": "May Prod" }
      ]
    }
  ],
  "statusRows": {
    "shipped": {
      "Jan": ["API design doc", "Auth provider selection"],
      "Feb": ["JWT middleware", "Rate limiting v1"],
      "Mar": ["Gateway routing", "Health checks"],
      "Apr": ["Load testing complete"]
    },
    "inProgress": {
      "Jan": [],
      "Feb": ["OAuth2 flows"],
      "Mar": ["Dashboard UI"],
      "Apr": ["E2E integration tests", "Canary deployment"]
    },
    "carryover": {
      "Jan": [],
      "Feb": [],
      "Mar": ["Logging pipeline"],
      "Apr": ["Metrics aggregation"]
    },
    "blockers": {
      "Jan": [],
      "Feb": [],
      "Mar": [],
      "Apr": ["Vendor SDK delay", "Perf regression in staging"]
    }
  }
}
```

### File Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── data.json
│   ├── data.template.json
│   ├── Components/
│   │   ├── App.razor
│   │   ├── Routes.razor
│   │   ├── Layout/
│   │   │   └── MainLayout.razor
│   │   └── Pages/
│   │       ├── Dashboard.razor
│   │       ├── Dashboard.razor.css
│   │       └── _Imports.razor
│   ├── Models/
│   │   └── DashboardData.cs
│   ├── Services/
│   │   └── DashboardDataService.cs
│   └── wwwroot/
│       └── app.css
└── README.md
```

### NuGet Packages Required

**None beyond what the template provides.** The default `blazor` template includes everything needed:

- `Microsoft.AspNetCore.Components.Web` (Blazor rendering)
- `System.Text.Json` (JSON deserialization)
- `Microsoft.Extensions.FileProviders.Physical` (file watching, built-in)

This is a zero-dependency project beyond the .NET 8 SDK itself.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/bb86bfec2df0d6bd7bbd5822dfaee699d01b8eb7/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
