# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 19:58 UTC_

### Summary

This project is a **single-page, local-only Blazor Server application** that renders an executive-friendly project status dashboard. It reads data from a `data.json` file and displays milestones, shipped work, in-progress items, carryover from previous months, and a visual timeline. The primary consumption model is **screenshots for PowerPoint decks**, which fundamentally shapes every technical decision: we optimize for visual clarity, print-readiness, and simplicity over interactivity or scale. ---

### Key Findings

- **Blazor Server is ideal for this use case**: server-side rendering means the full page is composed on the server and sent as HTML, which is perfect for a screenshot-oriented dashboard with no need for offline/client-side capability.
- **No database is needed**: a flat `data.json` file read via `System.Text.Json` is the simplest, most maintainable approach. The data volume (one project's milestones) is trivially small.
- **No authentication or authorization is warranted**: the app runs locally, likely on `localhost:5000`, for a single user. Adding auth would be over-engineering.
- **CSS is the primary "framework"**: the visual design quality depends entirely on clean HTML and well-crafted CSS. No UI component library is necessary—they add weight and fight your custom design.
- **Screenshot-friendliness requires specific design constraints**: fixed-width layouts (e.g., 1200px), high-contrast colors, large readable fonts (14px+ body, 18px+ headings), and avoidance of scroll-dependent content.
- **The original HTML design file is the single source of truth for visual design**: all layout and styling decisions should start from `OriginalDesignConcept.html` and the `ReportingDashboardDesign.png` mockup.
- **Hot reload (`dotnet watch`) provides excellent developer experience** in .NET 8 Blazor Server, enabling rapid iteration on the visual design.
- **.NET 8 is LTS (Long Term Support)** with support until November 2026, making it a stable foundation.
- ---
- **Scaffold the Blazor Server project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
- **Create data models** — `DashboardData.cs` with records matching the JSON structure
- **Create `DashboardDataService`** — reads and deserializes `data.json`
- **Build the `Dashboard.razor` page** — translate the original HTML design into Razor markup
- **Port/adapt the CSS** — from `OriginalDesignConcept.html` into `dashboard.css`
- **Create sample `data.json`** — fictional project with realistic executive-level data
- **Test visually** — load in browser, verify screenshot quality
- **Milestone timeline** — CSS Grid-based horizontal timeline at the top
- **Status indicators** — color-coded badges, progress bars, health indicators
- **Responsive fixed-width** — ensure 1200px centered layout screenshots consistently
- **Print stylesheet** — `@media print` rules for clean Ctrl+P output
- **Subtle refinements** — box shadows, border-radius, spacing, typography hierarchy
- **Auto-reload on file change** — `FileSystemWatcher` triggers Blazor re-render
- **CSS custom properties for theming** — easy color scheme changes
- **`data.template.json`** — documented template for creating new project reports
- **Multiple project support** — parameterized route (`/dashboard/{projectName}`) loading `{projectName}.json`
- **Start from the existing HTML**: Don't redesign from scratch. The `OriginalDesignConcept.html` already has proven layout and styling. Port it directly into Razor and iterate.
- **Use `dotnet watch`**: Hot reload makes CSS tweaking fast and satisfying.
- **Use browser DevTools**: Chrome's device toolbar can simulate exact screenshot dimensions for consistency.
- **CSS Grid for the timeline**: This is the single hardest visual element. Prototype it in isolation first (even in a plain HTML file) before integrating into Blazor.
- ❌ Admin panel for editing data (use a text editor)
- ❌ User accounts or login
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Docker containers
- ❌ CI/CD pipeline
- ❌ Logging infrastructure (console output is fine)
- ❌ Error pages or 404 handling (single page app, one route)
- ❌ Client-side Blazor/WASM compilation
- ---
- ```json
- {
- "project": {
- "name": "Project Atlas",
- "lead": "Jane Smith",
- "status": "On Track",
- "lastUpdated": "2026-04-10",
- "overallHealth": "on-track",
- "summary": "Cloud migration initiative progressing well. Phase 2 authentication module shipped ahead of schedule."
- },
- "milestones": [
- { "title": "Phase 1: Infrastructure", "date": "2026-01-15", "status": "completed", "description": "Core infrastructure provisioned" },
- { "title": "Phase 2: Auth Module", "date": "2026-03-01", "status": "completed", "description": "SSO and RBAC implemented" },
- { "title": "Phase 3: Data Migration", "date": "2026-05-15", "status": "in-progress", "description": "Legacy data migration to new schema" },
- { "title": "Phase 4: Launch", "date": "2026-07-01", "status": "upcoming", "description": "Production launch and cutover" }
- ],
- "shipped": [
- { "title": "SSO Integration", "description": "Single sign-on with Azure AD", "status": "completed", "category": "Security", "percentComplete": 100 },
- { "title": "Role-Based Access Control", "description": "Admin, Editor, Viewer roles", "status": "completed", "category": "Security", "percentComplete": 100 },
- { "title": "Monitoring Dashboard", "description": "Real-time system health monitoring", "status": "completed", "category": "Observability", "percentComplete": 100 }
- ],
- "inProgress": [
- { "title": "Data Migration Scripts", "description": "ETL pipeline for legacy database", "status": "in-progress", "category": "Data", "percentComplete": 65 },
- { "title": "API Gateway Setup", "description": "Rate limiting and routing configuration", "status": "in-progress", "category": "Infrastructure", "percentComplete": 40 }
- ],
- "carriedOver": [
- { "title": "Performance Testing", "description": "Load testing deferred from March due to environment availability", "status": "carried-over", "category": "Quality", "percentComplete": 10 }
- ],
- "currentMonth": {
- "totalItems": 6,
- "completedItems": 3,
- "carriedItems": 1,
- "overallHealth": "on-track"
- }
- }
- ```
- | Package | Version | Required? | Purpose |
- |---------|---------|-----------|---------|
- | `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | **Yes** | Blazor Server, Kestrel, Razor, System.Text.Json — everything needed |
- | *No additional NuGet packages required* | — | — | The entire project can be built with zero external dependencies |

### Recommended Tools & Technologies

- ---
- | Component | Recommendation | Version | Notes |
- |-----------|---------------|---------|-------|
- | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional package needed. Ships with `Microsoft.AspNetCore.App` |
- | **CSS Approach** | Custom CSS (no framework) | N/A | Tailored to match the HTML design template. Use CSS Grid for the timeline, Flexbox for cards/sections |
- | **CSS Reset** | `modern-normalize` | 2.0+ | Optional. A lightweight reset for cross-browser consistency. Can be vendored as a single CSS file |
- | **Icons** | Inline SVG or Unicode symbols | N/A | Avoid icon font libraries. Use ✅ ⚠️ 🔵 ● or simple inline SVGs for status indicators |
- | **Fonts** | System font stack | N/A | `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif` — no external font loading, instant rendering |
- | **Charts/Visuals** | Pure CSS progress bars + HTML/CSS timeline | N/A | No charting library needed. CSS `linear-gradient` and `width: X%` for progress bars. CSS Grid for milestone timeline |
- | **JavaScript** | None (or minimal) | N/A | Blazor Server handles interactivity. For a screenshot-oriented page, zero JS is achievable and preferred |
- Fight your custom design (the original HTML template has a specific look)
- Add 500KB+ of CSS/JS you don't need
- Make it harder to get pixel-perfect screenshots
- Introduce upgrade/compatibility concerns for a trivially simple page
- | Component | Recommendation | Version | Notes |
- |-----------|---------------|---------|-------|
- | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Preferred over Newtonsoft.Json for new .NET 8 projects. Zero additional dependency |
- | **Data Models** | C# Records | .NET 8 | Use `record` types for immutable data models. Clean, concise, perfect for deserialized config |
- | **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload data when `data.json` changes without restarting the app |
- | **Configuration** | Direct file read via `File.ReadAllTextAsync` | Built-in | Don't overuse `IConfiguration`/`appsettings.json` for this. A dedicated `data.json` with a typed model is cleaner |
- | Component | Recommendation | Version | Notes |
- |-----------|---------------|---------|-------|
- | **Runtime** | .NET 8 SDK | 8.0.x (latest patch) | LTS release, supported through Nov 2026 |
- | **Web Server** | Kestrel (built-in) | Ships with .NET 8 | No IIS, no reverse proxy, no Docker. Just `dotnet run` |
- | **IDE** | Visual Studio 2022 17.8+ or VS Code with C# Dev Kit | Latest | Both have excellent Blazor support with hot reload |
- | **Dev Experience** | `dotnet watch` | Built-in | Hot reload for `.razor`, `.css`, and `.cs` files |
- | Component | Recommendation | Version | Notes |
- |-----------|---------------|---------|-------|
- | **Unit Testing** | xUnit | 2.7+ | Only if you want to test JSON deserialization logic |
- | **Blazor Component Testing** | bUnit | 1.25+ | Only if you want to test the Razor component renders correctly |
- | **Snapshot Testing** | Verify | 23.x | Optional: snapshot-test the rendered HTML for regression detection |
- ```
- ReportingDashboard/
- ├── ReportingDashboard.sln
- ├── src/
- │   └── ReportingDashboard/
- │       ├── ReportingDashboard.csproj
- │       ├── Program.cs
- │       ├── Models/
- │       │   └── DashboardData.cs          # C# record types for data.json shape
- │       ├── Services/
- │       │   └── DashboardDataService.cs    # Reads and deserializes data.json
- │       ├── Components/
- │       │   ├── App.razor                  # Root component
- │       │   ├── Routes.razor
- │       │   └── Pages/
- │       │       └── Dashboard.razor        # The single page
- │       ├── wwwroot/
- │       │   ├── css/
- │       │   │   └── dashboard.css          # All custom styles
- │       │   └── data.json                  # Project data (or root-level)
- │       └── Properties/
- │           └── launchSettings.json
- └── data.json                              # Alternative: root-level for easy editing
- ```
- ---
- This is intentionally the simplest possible architecture. No layers of abstraction, no repository pattern, no CQRS, no mediator. The data flow is:
- ```
- data.json  →  DashboardDataService  →  Dashboard.razor  →  HTML/CSS
- (file)       (C# service, DI)        (Razor page)       (browser)
- ```
- The `data.json` structure should mirror what the dashboard displays. Recommended schema:
- ```csharp
- public record DashboardData
- {
- public ProjectInfo Project { get; init; }
- public List<Milestone> Milestones { get; init; } = [];
- public List<WorkItem> Shipped { get; init; } = [];
- public List<WorkItem> InProgress { get; init; } = [];
- public List<WorkItem> CarriedOver { get; init; } = [];
- public MonthSummary CurrentMonth { get; init; }
- }
- public record ProjectInfo(string Name, string Lead, string Status, string LastUpdated);
- public record Milestone(string Title, string Date, string Status, string Description);
- // Status: "completed" | "in-progress" | "upcoming" | "at-risk"
- public record WorkItem(string Title, string Description, string Status, string Category, int? PercentComplete);
- public record MonthSummary(int TotalItems, int CompletedItems, int CarriedItems, string OverallHealth);
- // OverallHealth: "on-track" | "at-risk" | "behind"
- ```
- ```csharp
- // Program.cs — keep it minimal
- var builder = WebApplication.CreateBuilder(args);
- builder.Services.AddRazorComponents().AddInteractiveServerComponents();
- builder.Services.AddSingleton<DashboardDataService>();
- var app = builder.Build();
- app.UseStaticFiles();
- app.UseAntiforgery();
- app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
- app.Run();
- ```
- ```csharp
- public class DashboardDataService
- {
- private readonly string _dataPath;
- public DashboardDataService(IWebHostEnvironment env)
- {
- _dataPath = Path.Combine(env.WebRootPath, "data.json");
- }
- public async Task<DashboardData> GetDashboardDataAsync()
- {
- var json = await File.ReadAllTextAsync(_dataPath);
- return JsonSerializer.Deserialize<DashboardData>(json,
- new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
- }
- }
- ```
- For a local single-user app, reading a small JSON file on each request is perfectly fine. No caching needed unless the file exceeds several MB (it won't).
- Key CSS decisions for a dashboard that will be screenshotted:
- **Fixed width**: `max-width: 1200px; margin: 0 auto;` — ensures consistent screenshots regardless of monitor size
- **Print-friendly colors**: High contrast, avoid thin light-gray text. Dark text on white/light backgrounds
- **No hover-dependent information**: Everything must be visible without mouse interaction
- **Status colors**: Use a consistent, accessible palette:
- Completed/Shipped: `#2E7D32` (green)
- In Progress: `#1565C0` (blue)
- At Risk: `#E65100` (orange)
- Blocked/Behind: `#C62828` (red)
- Upcoming: `#757575` (gray)
- **Timeline**: CSS Grid with `grid-template-columns` mapped to time periods. Each milestone as a positioned marker with a connecting line via `border-top` or `::before`/`::after` pseudo-elements
- **Cards/Sections**: Use `border-left: 4px solid [status-color]` for work item cards — this is a proven pattern for scannable status dashboards
- **`@media print`** CSS rules: Consider adding print styles so Ctrl+P also produces clean output
- ---

### Considerations & Risks

- This is a local-only tool running on `localhost`. Adding authentication would be:
- Contrary to the project goal of simplicity
- Unnecessary for a single-user, non-networked application
- A maintenance burden with zero security benefit (if someone has access to your localhost, they already have access to your machine)
- If in the future this needs to be shared on an intranet, the simplest addition would be Windows Authentication via `builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate()` — zero user management, leverages existing AD credentials.
- `data.json` contains project status data, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of source control via `.gitignore`.
- | Aspect | Recommendation |
- |--------|---------------|
- | **How to run** | `dotnet run` from the project directory |
- | **Port** | `http://localhost:5000` (configure in `launchSettings.json`) |
- | **Deployment** | None. This runs locally. If sharing is needed later: `dotnet publish -c Release` and xcopy the output |
- | **Containerization** | Not needed. This adds complexity for zero benefit in a local-only scenario |
- | **HTTPS** | Not needed for localhost. Kestrel defaults can be set to HTTP-only to avoid certificate warnings |
- ---
- | Risk | Likelihood | Impact | Mitigation |
- |------|-----------|--------|------------|
- | Adding a database | Medium | Weeks of unnecessary work | `data.json` is the database. Resist the urge. |
- | Adding a UI component library | Medium | Design fights, bloated output | Custom CSS matching the original HTML design |
- | Adding authentication | Low | Complexity, dev friction | Not needed for local use |
- | Adding client-side Blazor (WASM) | Low | Larger download, slower startup | Blazor Server is correct for this |
- | Building an admin UI to edit data | Medium | Scope creep | Edit `data.json` in any text editor or VS Code |
- Blazor Server maintains a real-time SignalR connection. For a screenshot-oriented page this is slight overkill—Static Server-Side Rendering (SSR) in .NET 8 would also work. However, Blazor Server is the specified stack and works perfectly fine. The SignalR connection has no negative impact for local use.
- As the dashboard evolves, the `data.json` structure may need new fields. Mitigation:
- Use nullable properties (`string? NewField`) for backward compatibility
- Keep a `data.template.json` with all fields documented
- Use `JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }` so missing fields don't cause errors
- If screenshots are taken in different browsers, rendering may vary slightly. Mitigation:
- Standardize on one browser for screenshots (recommend Chrome/Edge)
- Test the fixed-width layout in that browser
- Avoid browser-specific CSS features
- | Skill | Required Level | Notes |
- |-------|---------------|-------|
- | C# / .NET 8 | Basic | Minimal code; mostly model classes and one service |
- | Razor syntax | Basic | Single page with `@foreach` loops and `@if` conditionals |
- | HTML/CSS | Intermediate | This is where most of the work lives. The visual quality depends on CSS craftsmanship |
- | JavaScript | None | Not needed |
- | Blazor concepts | Basic | Component lifecycle, dependency injection, `OnInitializedAsync` |
- ---
- **Where should `data.json` live?** Options: `wwwroot/` (accessible via URL, editable), project root (requires path configuration), or a configurable path via `appsettings.json`. **Recommendation:** `wwwroot/data/data.json` for simplicity, with an `appsettings.json` override option.
- **Should the page auto-refresh when `data.json` changes?** A `FileSystemWatcher` + SignalR push is trivial with Blazor Server. Nice for iterating on the data, but not strictly necessary. **Recommendation:** Yes, implement it—it's ~15 lines of code and significantly improves the editing experience.
- **What time granularity for the milestone timeline?** Monthly? Weekly? Quarterly? This affects the CSS Grid layout. **Recommendation:** Monthly columns with the ability to span quarters, matching typical executive reporting cadence.
- **Should there be a "last updated" timestamp?** Executives often ask "is this current?" **Recommendation:** Yes, display the file's last-modified timestamp in the page footer.
- **Multiple projects?** Currently scoped to a single project per `data.json`. If multiple projects are needed later, the simplest extension is multiple JSON files with a dropdown or multiple pages. **Recommendation:** Design the data model for one project now; don't prematurely generalize.
- **Color theme / branding?** Should the dashboard match a specific corporate color scheme or PowerPoint template? **Recommendation:** Use CSS custom properties (`--color-primary`, `--color-success`, etc.) so theming is a 5-minute change.
- ---
- ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Project Reporting Dashboard
**Stack:** C# .NET 8 / Blazor Server / Local-Only / .sln Structure
**Date:** 2026-04-10
**Status:** Final Research

---

## 1. Executive Summary

This project is a **single-page, local-only Blazor Server application** that renders an executive-friendly project status dashboard. It reads data from a `data.json` file and displays milestones, shipped work, in-progress items, carryover from previous months, and a visual timeline. The primary consumption model is **screenshots for PowerPoint decks**, which fundamentally shapes every technical decision: we optimize for visual clarity, print-readiness, and simplicity over interactivity or scale.

**Primary Recommendation:** A minimal Blazor Server app with zero authentication, no database, no cloud dependencies—just a single `.razor` page that deserializes `data.json` using `System.Text.Json`, renders styled HTML/CSS, and runs on Kestrel locally. Use CSS Grid/Flexbox for layout, keep JavaScript to an absolute minimum (or zero), and lean on clean semantic HTML that screenshots beautifully. The entire solution should be buildable and runnable with `dotnet run` in under 5 seconds.

---

## 2. Key Findings

- **Blazor Server is ideal for this use case**: server-side rendering means the full page is composed on the server and sent as HTML, which is perfect for a screenshot-oriented dashboard with no need for offline/client-side capability.
- **No database is needed**: a flat `data.json` file read via `System.Text.Json` is the simplest, most maintainable approach. The data volume (one project's milestones) is trivially small.
- **No authentication or authorization is warranted**: the app runs locally, likely on `localhost:5000`, for a single user. Adding auth would be over-engineering.
- **CSS is the primary "framework"**: the visual design quality depends entirely on clean HTML and well-crafted CSS. No UI component library is necessary—they add weight and fight your custom design.
- **Screenshot-friendliness requires specific design constraints**: fixed-width layouts (e.g., 1200px), high-contrast colors, large readable fonts (14px+ body, 18px+ headings), and avoidance of scroll-dependent content.
- **The original HTML design file is the single source of truth for visual design**: all layout and styling decisions should start from `OriginalDesignConcept.html` and the `ReportingDashboardDesign.png` mockup.
- **Hot reload (`dotnet watch`) provides excellent developer experience** in .NET 8 Blazor Server, enabling rapid iteration on the visual design.
- **.NET 8 is LTS (Long Term Support)** with support until November 2026, making it a stable foundation.

---

## 3. Recommended Technology Stack

### Frontend (Rendering Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional package needed. Ships with `Microsoft.AspNetCore.App` |
| **CSS Approach** | Custom CSS (no framework) | N/A | Tailored to match the HTML design template. Use CSS Grid for the timeline, Flexbox for cards/sections |
| **CSS Reset** | `modern-normalize` | 2.0+ | Optional. A lightweight reset for cross-browser consistency. Can be vendored as a single CSS file |
| **Icons** | Inline SVG or Unicode symbols | N/A | Avoid icon font libraries. Use ✅ ⚠️ 🔵 ● or simple inline SVGs for status indicators |
| **Fonts** | System font stack | N/A | `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif` — no external font loading, instant rendering |
| **Charts/Visuals** | Pure CSS progress bars + HTML/CSS timeline | N/A | No charting library needed. CSS `linear-gradient` and `width: X%` for progress bars. CSS Grid for milestone timeline |
| **JavaScript** | None (or minimal) | N/A | Blazor Server handles interactivity. For a screenshot-oriented page, zero JS is achievable and preferred |

**Why no UI component library (MudBlazor, Radzen, etc.)?** These libraries are excellent for enterprise CRUD apps, but for this project they would:
1. Fight your custom design (the original HTML template has a specific look)
2. Add 500KB+ of CSS/JS you don't need
3. Make it harder to get pixel-perfect screenshots
4. Introduce upgrade/compatibility concerns for a trivially simple page

### Backend (Data Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Preferred over Newtonsoft.Json for new .NET 8 projects. Zero additional dependency |
| **Data Models** | C# Records | .NET 8 | Use `record` types for immutable data models. Clean, concise, perfect for deserialized config |
| **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload data when `data.json` changes without restarting the app |
| **Configuration** | Direct file read via `File.ReadAllTextAsync` | Built-in | Don't overuse `IConfiguration`/`appsettings.json` for this. A dedicated `data.json` with a typed model is cleaner |

### Infrastructure / Runtime

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | .NET 8 SDK | 8.0.x (latest patch) | LTS release, supported through Nov 2026 |
| **Web Server** | Kestrel (built-in) | Ships with .NET 8 | No IIS, no reverse proxy, no Docker. Just `dotnet run` |
| **IDE** | Visual Studio 2022 17.8+ or VS Code with C# Dev Kit | Latest | Both have excellent Blazor support with hot reload |
| **Dev Experience** | `dotnet watch` | Built-in | Hot reload for `.razor`, `.css`, and `.cs` files |

### Testing (Minimal)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | Only if you want to test JSON deserialization logic |
| **Blazor Component Testing** | bUnit | 1.25+ | Only if you want to test the Razor component renders correctly |
| **Snapshot Testing** | Verify | 23.x | Optional: snapshot-test the rendered HTML for regression detection |

**Honest recommendation:** For a simple single-page dashboard, manual testing by visual inspection is sufficient. Don't add test infrastructure unless the project grows.

### Project Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   └── DashboardData.cs          # C# record types for data.json shape
│       ├── Services/
│       │   └── DashboardDataService.cs    # Reads and deserializes data.json
│       ├── Components/
│       │   ├── App.razor                  # Root component
│       │   ├── Routes.razor
│       │   └── Pages/
│       │       └── Dashboard.razor        # The single page
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css          # All custom styles
│       │   └── data.json                  # Project data (or root-level)
│       └── Properties/
│           └── launchSettings.json
└── data.json                              # Alternative: root-level for easy editing
```

---

## 4. Architecture Recommendations

### Pattern: Flat File → Service → Component

This is intentionally the simplest possible architecture. No layers of abstraction, no repository pattern, no CQRS, no mediator. The data flow is:

```
data.json  →  DashboardDataService  →  Dashboard.razor  →  HTML/CSS
   (file)       (C# service, DI)        (Razor page)       (browser)
```

### Data Model Design

The `data.json` structure should mirror what the dashboard displays. Recommended schema:

```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
    public List<WorkItem> Shipped { get; init; } = [];
    public List<WorkItem> InProgress { get; init; } = [];
    public List<WorkItem> CarriedOver { get; init; } = [];
    public MonthSummary CurrentMonth { get; init; }
}

public record ProjectInfo(string Name, string Lead, string Status, string LastUpdated);

public record Milestone(string Title, string Date, string Status, string Description);
// Status: "completed" | "in-progress" | "upcoming" | "at-risk"

public record WorkItem(string Title, string Description, string Status, string Category, int? PercentComplete);

public record MonthSummary(int TotalItems, int CompletedItems, int CarriedItems, string OverallHealth);
// OverallHealth: "on-track" | "at-risk" | "behind"
```

### Service Registration (Minimal DI)

```csharp
// Program.cs — keep it minimal
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### Data Loading Strategy

**Recommended approach:** Read `data.json` on each page load (or cache with file-watcher invalidation).

```csharp
public class DashboardDataService
{
    private readonly string _dataPath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.WebRootPath, "data.json");
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var json = await File.ReadAllTextAsync(_dataPath);
        return JsonSerializer.Deserialize<DashboardData>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
```

For a local single-user app, reading a small JSON file on each request is perfectly fine. No caching needed unless the file exceeds several MB (it won't).

### CSS Architecture for Screenshot-Friendliness

Key CSS decisions for a dashboard that will be screenshotted:

1. **Fixed width**: `max-width: 1200px; margin: 0 auto;` — ensures consistent screenshots regardless of monitor size
2. **Print-friendly colors**: High contrast, avoid thin light-gray text. Dark text on white/light backgrounds
3. **No hover-dependent information**: Everything must be visible without mouse interaction
4. **Status colors**: Use a consistent, accessible palette:
   - Completed/Shipped: `#2E7D32` (green)
   - In Progress: `#1565C0` (blue)
   - At Risk: `#E65100` (orange)
   - Blocked/Behind: `#C62828` (red)
   - Upcoming: `#757575` (gray)
5. **Timeline**: CSS Grid with `grid-template-columns` mapped to time periods. Each milestone as a positioned marker with a connecting line via `border-top` or `::before`/`::after` pseudo-elements
6. **Cards/Sections**: Use `border-left: 4px solid [status-color]` for work item cards — this is a proven pattern for scannable status dashboards
7. **`@media print`** CSS rules: Consider adding print styles so Ctrl+P also produces clean output

---

## 5. Security & Infrastructure

### Authentication & Authorization

**Recommendation: None.**

This is a local-only tool running on `localhost`. Adding authentication would be:
- Contrary to the project goal of simplicity
- Unnecessary for a single-user, non-networked application
- A maintenance burden with zero security benefit (if someone has access to your localhost, they already have access to your machine)

If in the future this needs to be shared on an intranet, the simplest addition would be Windows Authentication via `builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate()` — zero user management, leverages existing AD credentials.

### Data Protection

- `data.json` contains project status data, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of source control via `.gitignore`.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **How to run** | `dotnet run` from the project directory |
| **Port** | `http://localhost:5000` (configure in `launchSettings.json`) |
| **Deployment** | None. This runs locally. If sharing is needed later: `dotnet publish -c Release` and xcopy the output |
| **Containerization** | Not needed. This adds complexity for zero benefit in a local-only scenario |
| **HTTPS** | Not needed for localhost. Kestrel defaults can be set to HTTP-only to avoid certificate warnings |

### Infrastructure Costs

**$0.** This runs on the developer's existing machine. No cloud, no servers, no licenses beyond what's already available (Visual Studio Community or VS Code are free).

---

## 6. Risks & Trade-offs

### Risk: Over-Engineering

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Adding a database | Medium | Weeks of unnecessary work | `data.json` is the database. Resist the urge. |
| Adding a UI component library | Medium | Design fights, bloated output | Custom CSS matching the original HTML design |
| Adding authentication | Low | Complexity, dev friction | Not needed for local use |
| Adding client-side Blazor (WASM) | Low | Larger download, slower startup | Blazor Server is correct for this |
| Building an admin UI to edit data | Medium | Scope creep | Edit `data.json` in any text editor or VS Code |

### Risk: Blazor Server SignalR Dependency

Blazor Server maintains a real-time SignalR connection. For a screenshot-oriented page this is slight overkill—Static Server-Side Rendering (SSR) in .NET 8 would also work. However, Blazor Server is the specified stack and works perfectly fine. The SignalR connection has no negative impact for local use.

**Trade-off accepted:** Blazor Server's SignalR connection means the page goes "stale" if the server stops. For screenshot purposes this is irrelevant—you load the page, screenshot it, done.

### Risk: JSON Schema Evolution

As the dashboard evolves, the `data.json` structure may need new fields. Mitigation:
- Use nullable properties (`string? NewField`) for backward compatibility
- Keep a `data.template.json` with all fields documented
- Use `JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }` so missing fields don't cause errors

### Risk: CSS Cross-Browser Screenshot Inconsistency

If screenshots are taken in different browsers, rendering may vary slightly. Mitigation:
- Standardize on one browser for screenshots (recommend Chrome/Edge)
- Test the fixed-width layout in that browser
- Avoid browser-specific CSS features

### Skills Assessment

| Skill | Required Level | Notes |
|-------|---------------|-------|
| C# / .NET 8 | Basic | Minimal code; mostly model classes and one service |
| Razor syntax | Basic | Single page with `@foreach` loops and `@if` conditionals |
| HTML/CSS | Intermediate | This is where most of the work lives. The visual quality depends on CSS craftsmanship |
| JavaScript | None | Not needed |
| Blazor concepts | Basic | Component lifecycle, dependency injection, `OnInitializedAsync` |

---

## 7. Open Questions

1. **Where should `data.json` live?** Options: `wwwroot/` (accessible via URL, editable), project root (requires path configuration), or a configurable path via `appsettings.json`. **Recommendation:** `wwwroot/data/data.json` for simplicity, with an `appsettings.json` override option.

2. **Should the page auto-refresh when `data.json` changes?** A `FileSystemWatcher` + SignalR push is trivial with Blazor Server. Nice for iterating on the data, but not strictly necessary. **Recommendation:** Yes, implement it—it's ~15 lines of code and significantly improves the editing experience.

3. **What time granularity for the milestone timeline?** Monthly? Weekly? Quarterly? This affects the CSS Grid layout. **Recommendation:** Monthly columns with the ability to span quarters, matching typical executive reporting cadence.

4. **Should there be a "last updated" timestamp?** Executives often ask "is this current?" **Recommendation:** Yes, display the file's last-modified timestamp in the page footer.

5. **Multiple projects?** Currently scoped to a single project per `data.json`. If multiple projects are needed later, the simplest extension is multiple JSON files with a dropdown or multiple pages. **Recommendation:** Design the data model for one project now; don't prematurely generalize.

6. **Color theme / branding?** Should the dashboard match a specific corporate color scheme or PowerPoint template? **Recommendation:** Use CSS custom properties (`--color-primary`, `--color-success`, etc.) so theming is a 5-minute change.

---

## 8. Implementation Recommendations

### Phase 1: Core Dashboard (MVP) — ~2-4 hours

**Goal:** A working page that reads `data.json` and renders all sections matching the original HTML design.

1. **Scaffold the Blazor Server project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
2. **Create data models** — `DashboardData.cs` with records matching the JSON structure
3. **Create `DashboardDataService`** — reads and deserializes `data.json`
4. **Build the `Dashboard.razor` page** — translate the original HTML design into Razor markup
5. **Port/adapt the CSS** — from `OriginalDesignConcept.html` into `dashboard.css`
6. **Create sample `data.json`** — fictional project with realistic executive-level data
7. **Test visually** — load in browser, verify screenshot quality

### Phase 2: Visual Polish — ~1-2 hours

1. **Milestone timeline** — CSS Grid-based horizontal timeline at the top
2. **Status indicators** — color-coded badges, progress bars, health indicators
3. **Responsive fixed-width** — ensure 1200px centered layout screenshots consistently
4. **Print stylesheet** — `@media print` rules for clean Ctrl+P output
5. **Subtle refinements** — box shadows, border-radius, spacing, typography hierarchy

### Phase 3: Quality of Life (Optional) — ~1 hour

1. **Auto-reload on file change** — `FileSystemWatcher` triggers Blazor re-render
2. **CSS custom properties for theming** — easy color scheme changes
3. **`data.template.json`** — documented template for creating new project reports
4. **Multiple project support** — parameterized route (`/dashboard/{projectName}`) loading `{projectName}.json`

### Quick Wins

- **Start from the existing HTML**: Don't redesign from scratch. The `OriginalDesignConcept.html` already has proven layout and styling. Port it directly into Razor and iterate.
- **Use `dotnet watch`**: Hot reload makes CSS tweaking fast and satisfying.
- **Use browser DevTools**: Chrome's device toolbar can simulate exact screenshot dimensions for consistency.
- **CSS Grid for the timeline**: This is the single hardest visual element. Prototype it in isolation first (even in a plain HTML file) before integrating into Blazor.

### What NOT to Build

- ❌ Admin panel for editing data (use a text editor)
- ❌ User accounts or login
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Docker containers
- ❌ CI/CD pipeline
- ❌ Logging infrastructure (console output is fine)
- ❌ Error pages or 404 handling (single page app, one route)
- ❌ Client-side Blazor/WASM compilation

---

## Appendix A: Recommended `data.json` Example Structure

```json
{
  "project": {
    "name": "Project Atlas",
    "lead": "Jane Smith",
    "status": "On Track",
    "lastUpdated": "2026-04-10",
    "overallHealth": "on-track",
    "summary": "Cloud migration initiative progressing well. Phase 2 authentication module shipped ahead of schedule."
  },
  "milestones": [
    { "title": "Phase 1: Infrastructure", "date": "2026-01-15", "status": "completed", "description": "Core infrastructure provisioned" },
    { "title": "Phase 2: Auth Module", "date": "2026-03-01", "status": "completed", "description": "SSO and RBAC implemented" },
    { "title": "Phase 3: Data Migration", "date": "2026-05-15", "status": "in-progress", "description": "Legacy data migration to new schema" },
    { "title": "Phase 4: Launch", "date": "2026-07-01", "status": "upcoming", "description": "Production launch and cutover" }
  ],
  "shipped": [
    { "title": "SSO Integration", "description": "Single sign-on with Azure AD", "status": "completed", "category": "Security", "percentComplete": 100 },
    { "title": "Role-Based Access Control", "description": "Admin, Editor, Viewer roles", "status": "completed", "category": "Security", "percentComplete": 100 },
    { "title": "Monitoring Dashboard", "description": "Real-time system health monitoring", "status": "completed", "category": "Observability", "percentComplete": 100 }
  ],
  "inProgress": [
    { "title": "Data Migration Scripts", "description": "ETL pipeline for legacy database", "status": "in-progress", "category": "Data", "percentComplete": 65 },
    { "title": "API Gateway Setup", "description": "Rate limiting and routing configuration", "status": "in-progress", "category": "Infrastructure", "percentComplete": 40 }
  ],
  "carriedOver": [
    { "title": "Performance Testing", "description": "Load testing deferred from March due to environment availability", "status": "carried-over", "category": "Quality", "percentComplete": 10 }
  ],
  "currentMonth": {
    "totalItems": 6,
    "completedItems": 3,
    "carriedItems": 1,
    "overallHealth": "on-track"
  }
}
```

## Appendix B: Key NuGet Packages (Exhaustive List)

| Package | Version | Required? | Purpose |
|---------|---------|-----------|---------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | **Yes** | Blazor Server, Kestrel, Razor, System.Text.Json — everything needed |
| *No additional NuGet packages required* | — | — | The entire project can be built with zero external dependencies |

**This is a feature, not a limitation.** Zero external dependencies means zero supply-chain risk, zero version conflicts, zero maintenance burden, and zero `dotnet restore` wait time. The .NET 8 framework reference provides everything this project needs.

---

*End of Research Document*
