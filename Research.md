# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-11 16:51 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed for local-only use. It reads project data from a `data.json` file and renders a timeline/milestone view with a heatmap grid showing shipped, in-progress, carryover, and blocker items. The design is intentionally simple—optimized for taking screenshots for PowerPoint decks. **Primary recommendation:** Use a minimal Blazor Server app with zero external JavaScript dependencies. Render the SVG timeline natively in Razor components, use CSS Grid/Flexbox for the heatmap layout (matching the reference HTML exactly), and deserialize `data.json` with `System.Text.Json`. No database, no authentication, no cloud services. The entire solution should be a single `.sln` with one Blazor Server project, deployable via `dotnet run`. ---

### Key Findings

- The reference `OriginalDesignConcept.html` is a static, self-contained HTML page with inline CSS and SVG—this translates almost 1:1 into Blazor Razor components with no framework overhead.
- The design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** extensively—both are natively supported in Blazor without any CSS framework dependency.
- The SVG timeline section is hand-authored markup—Blazor can render this directly in Razor with data binding, no charting library needed.
- The color palette is hardcoded and category-based (green=shipped, blue=in-progress, amber=carryover, red=blockers)—this maps cleanly to a simple enum-to-CSS-class mapping.
- The target resolution is 1920×1080 (fixed viewport for screenshots)—responsive design is unnecessary and should be avoided to maintain pixel-perfect screenshot fidelity.
- `System.Text.Json` in .NET 8 is mature and performant enough for deserializing a small config file; no need for Newtonsoft.Json.
- No authentication, no database, no external APIs—this dramatically simplifies the architecture to essentially a file-reading static renderer.
- Blazor Server's SignalR circuit is overkill for this use case, but it's the mandated stack and still works perfectly for local use with negligible overhead.
- Hot reload in .NET 8 Blazor Server works well, enabling fast iteration on layout and styling. --- **Goal:** Pixel-perfect replica of `OriginalDesignConcept.html` as a Blazor Server app, reading from `data.json`.
- **Scaffold the project** — `dotnet new blazorserver -n ReportingDashboard -f net8.0`
- **Define the data model** — Create C# POCOs matching the `data.json` schema above.
- **Create `data.json`** — Populate with fictional project data (e.g., "Project Phoenix" with milestones for a CRM migration).
- **Build the `DashboardDataService`** — Read and deserialize `data.json` on startup, register as singleton.
- **Port the CSS** — Copy the reference HTML's `<style>` block into `app.css` with minimal modifications (replace `body` fixed dimensions, keep all `.hm-*`, `.ship-*`, `.prog-*`, etc. classes).
- **Build components top-down:**
- `MainLayout.razor` — Remove default nav sidebar, use full-width layout
- `Header.razor` — Title, subtitle, legend icons
- `Timeline.razor` — SVG with data-bound milestone rendering
- `Heatmap.razor` — CSS Grid container with category rows
- **Test visually** — Open in Edge at 1920×1080, compare against reference design side-by-side.
- **Take a screenshot** — Verify it looks presentation-ready.
- Add `FileSystemWatcher` to auto-reload `data.json` when it changes (enables editing JSON while the app is running).
- Add subtle CSS transitions on hover for heatmap cells (optional flair for live demos).
- Add a print stylesheet (`@media print`) optimized for PDF export.
- Extract color palette into CSS custom properties (`--color-shipped: #34A853`) for easy theming.
- Add a route parameter: `/dashboard/{projectName}`
- Load `{projectName}.json` from `wwwroot/data/`
- Add a simple project selector dropdown or landing page
- **Fastest path to value:** Copy the reference HTML's CSS verbatim into `app.css`. This eliminates all design guesswork and ensures pixel-perfect fidelity from the start.
- **Use `dotnet watch`:** Enables hot reload—change a Razor component, see it instantly in the browser. Critical for iterating on SVG positioning.
- **Hardcode first, abstract later:** Get the page rendering with hardcoded data, then wire up `data.json`. Avoids debugging JSON parsing and layout issues simultaneously.
```xml
<!-- ReportingDashboard.csproj — the only package references needed -->
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="8.0.*" />
<!-- That's it. No other packages needed for MVP. -->
``` Actually, for a standard Blazor Server project created with `dotnet new blazorserver`, **zero additional NuGet packages are required**. Everything needed (`System.Text.Json`, Kestrel, Razor components, CSS isolation) is included in the default `Microsoft.NET.Sdk.Web` SDK.
```powershell
# Create solution and project
dotnet new sln -n ReportingDashboard
dotnet new blazorserver -n ReportingDashboard -f net8.0
dotnet sln add ReportingDashboard/ReportingDashboard.csproj

# Run with hot reload
cd ReportingDashboard
dotnet watch run

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
``` --- | Technology | Why Not | |-----------|---------| | **MudBlazor / Radzen** | The design is fully custom. These libraries impose their own design language and would require overriding most defaults. More work, not less. | | **Chart.js / ApexCharts** | Requires JS interop. The timeline SVG is simple enough to render natively. Custom milestone shapes (diamonds, circles) are easier in raw SVG than chart library customization. | | **Blazor WebAssembly** | Requires downloading the .NET runtime to the browser. Slower startup. No benefit for a local-only, single-user app. | | **Blazor Static SSR** | Viable alternative within .NET 8, but Server mode has better tooling support and the team is already familiar with it. | | **Tailwind CSS** | Requires a build pipeline (PostCSS). The reference design already has complete CSS. Adding Tailwind would be overhead with no benefit. | | **SQLite / LiteDB** | No persistence needed. JSON file is sufficient for read-only data that changes monthly. | | **Docker** | Single-user local app. `dotnet run` is simpler than Docker for this use case. |

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Mandated stack. Use Razor components, no JS interop needed. | | **CSS Strategy** | Scoped CSS (`.razor.css`) + global `app.css` | Built-in | Use Blazor's CSS isolation for component styles. Global styles for the color palette and grid layout. | | **CSS Layout** | CSS Grid + Flexbox | Native | Matches reference design exactly. No CSS framework (Bootstrap, Tailwind) needed or recommended. | | **SVG Rendering** | Inline SVG in Razor | Native | Render timeline directly in `.razor` files with `@foreach` loops over milestone data. No charting library. | | **Font** | Segoe UI | System font | Already on Windows. No web font loading needed. | | **Icons** | None | — | Design uses CSS shapes (rotated squares for diamonds, circles). No icon library needed. | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializer.Deserialize<T>()` with source generators for AOT-friendly serialization if desired. | | **File Reading** | `System.IO.File.ReadAllTextAsync` | Built-in | Read `data.json` from a known path (e.g., `wwwroot/data/data.json` or app root). | | **Configuration** | `IConfiguration` or direct file read | Built-in | For a simple app, direct file read is cleaner than wiring up `IConfiguration` for a single JSON file. | | **Data Caching** | `IMemoryCache` or singleton service | `Microsoft.Extensions.Caching.Memory` (built-in) | Cache parsed data in memory. Reload on file change via `FileSystemWatcher` if desired. | **None.** This project reads from a flat `data.json` file. No database is needed or recommended. If future requirements demand persistence (e.g., editing data through the UI), use **SQLite** via `Microsoft.Data.Sqlite` (3.x) or **LiteDB** (5.0.x) as an embedded document store—but this is explicitly out of scope. | Tool | Version | Purpose | |------|---------|---------| | **xUnit** | 2.7.x | Unit testing framework for .NET | | **bUnit** | 1.28.x+ | Blazor component testing (renders components in-memory) | | **FluentAssertions** | 6.12.x | Readable test assertions | | **Moq** | 4.20.x | Mocking (if services are injected) | Given the simplicity of this app, testing is low priority. If tests are written, focus on:
- JSON deserialization correctness (does `data.json` parse into the expected model?)
- SVG coordinate calculation logic (are milestones positioned correctly on the timeline?) | Tool | Version | Purpose | |------|---------|---------| | **.NET SDK** | 8.0.x LTS | Build, run, publish | | **Visual Studio 2022** or **VS Code + C# Dev Kit** | Latest | IDE | | **dotnet watch** | Built-in | Hot reload during development | ---
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── Models/
    │   ├── DashboardData.cs          // Root model
    │   ├── Milestone.cs              // Timeline milestones
    │   ├── HeatmapCategory.cs        // Shipped/InProgress/Carryover/Blockers
    │   └── HeatmapItem.cs            // Individual items in cells
    ├── Services/
    │   └── DashboardDataService.cs   // Reads and caches data.json
    ├── Components/
    │   ├── Layout/
    │   │   └── MainLayout.razor      // Full-page layout (no nav, no sidebar)
    │   ├── Pages/
    │   │   └── Dashboard.razor       // The single page
    │   ├── Header.razor              // Title bar + legend
    │   ├── Timeline.razor            // SVG milestone timeline
    │   ├── TimelineMilestone.razor   // Individual milestone marker
    │   ├── Heatmap.razor             // CSS Grid heatmap container
    │   ├── HeatmapRow.razor          // Single category row
    │   └── HeatmapCell.razor         // Single month×category cell
    ├── wwwroot/
    │   ├── css/
    │   │   └── app.css               // Global styles, color palette
    │   └── data/
    │       └── data.json             // Project data file
    └── Properties/
        └── launchSettings.json
``` **1. No component library.** MudBlazor, Radzen, and Syncfusion are all overkill. The design is custom CSS Grid + SVG. A component library would fight the design rather than help it. **2. No JavaScript interop.** The entire design is achievable with pure Razor + CSS + inline SVG. Avoid `IJSRuntime` calls entirely. **3. SVG timeline rendering in Razor.** The reference HTML hand-codes SVG elements. In Blazor, use a `Timeline.razor` component that:
- Accepts a list of `Milestone` objects
- Calculates X positions based on date-to-pixel mapping
- Renders `<line>`, `<circle>`, `<polygon>`, and `<text>` elements via `@foreach`
- Renders the "NOW" indicator line based on `DateTime.Now`
```csharp
// Example: date-to-X-position calculation
private double DateToX(DateTime date)
{
    var totalDays = (EndDate - StartDate).TotalDays;
    var elapsed = (date - StartDate).TotalDays;
    return (elapsed / totalDays) * SvgWidth;
}
``` **4. CSS class mapping by category.** Use an enum:
```csharp
public enum CategoryType { Shipped, InProgress, Carryover, Blockers }
``` Map to CSS classes: `ship-cell`, `prog-cell`, `carry-cell`, `block-cell`—directly matching the reference HTML's class names. **5. Fixed 1920×1080 viewport.** Add to `app.css`:
```css
body { width: 1920px; height: 1080px; overflow: hidden; }
``` This ensures screenshot consistency. Do not add responsive breakpoints.
```
data.json (file on disk)
    → DashboardDataService.LoadAsync()
        → System.Text.Json deserialization
            → DashboardData model (in-memory)
                → Injected into Dashboard.razor
                    → Passed as parameters to child components
                        → Rendered as HTML/SVG/CSS
```
```json
{
  "title": "Privacy Automation Release Roadmap",
  "organization": "Trusted Platform",
  "workstream": "Privacy Automation Workstream",
  "currentMonth": "April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "months": ["January", "February", "March", "April"],
  "currentMonthIndex": 3,
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      }
    ]
  },
  "heatmap": {
    "shipped": {
      "january": ["Item A", "Item B"],
      "february": ["Item C"],
      "march": ["Item D", "Item E"],
      "april": ["Item F"]
    },
    "inProgress": { ... },
    "carryover": { ... },
    "blockers": { ... }
  }
}
``` ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The app runs locally, accessed via `https://localhost:5001` or `http://localhost:5000`. No login, no roles, no middleware. If this ever needs to be shared on a network, the simplest addition would be a shared secret via a query string parameter or basic HTTP header check—but this is not recommended for the current scope.
- `data.json` contains project status data, not PII or secrets. No encryption needed.
- If the JSON contains sensitive project names, ensure the machine running the app has appropriate access controls (standard OS-level file permissions). | Aspect | Recommendation | |--------|---------------| | **Runtime** | Self-contained single-file publish: `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` | | **Hosting** | Kestrel (built into ASP.NET Core). No IIS, no reverse proxy, no Docker needed. | | **Launch** | `dotnet run` or double-click the published `.exe` | | **Port** | Configure in `launchSettings.json` or `appsettings.json`. Default `https://localhost:5001`. | | **HTTPS** | Use the .NET dev cert (`dotnet dev-certs https --trust`) for local use. Optional—HTTP is fine for localhost screenshots. | **$0.** This runs on the developer's local machine. No cloud, no hosting, no CI/CD pipeline needed for MVP. --- | Risk | Impact | Mitigation | |------|--------|------------| | **Blazor Server is heavier than needed** | Slight overhead from SignalR circuit for what is essentially a static page | Acceptable for local use. Alternative would be Blazor Static SSR (new in .NET 8), but Server mode is simpler to set up and the mandated stack. | | **JSON schema drift** | If `data.json` structure changes, deserialization silently produces nulls | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and validate required fields on load. | | **SVG coordinate math** | Timeline positioning bugs if date ranges change | Unit test the `DateToX()` mapping function. | | **Browser rendering differences** | Screenshots may vary across browsers | Standardize on **Edge** (Chromium) for all screenshots. Document this. | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | No component library | Must hand-code all UI | Design is too custom for MudBlazor/Radzen to help. Hand-coding is faster here. | | No database | Can't edit data through UI | Editing `data.json` in a text editor or VS Code is sufficient for the update cadence (monthly). | | Fixed 1920×1080 | Not usable on mobile/tablets | Explicitly designed for screenshots, not interactive use. | | No JS charting library | Must build SVG timeline manually | Chart.js or ApexCharts would require JS interop and wouldn't match the custom diamond/circle milestone markers from the design. | | Blazor Server over Static SSR | Persistent SignalR connection for a read-only page | Server mode is simpler to configure and the team is familiar with it. Static SSR (RenderMode.Static) could be explored later. |
- Scalability: Single user, local machine. Not a concern.
- Performance: Tiny JSON file, simple DOM. Not a concern.
- Availability: If it's down, run `dotnet run` again. Not a concern.
- Multi-tenancy: Single project dashboard. Not a concern. --- | # | Question | Stakeholder | Impact | |---|----------|-------------|--------| | 1 | **How often will `data.json` be updated?** If monthly, manual editing is fine. If weekly+, consider a simple edit form in the UI. | Project Manager | Determines if CRUD UI is needed | | 2 | **Should the "NOW" line auto-update to today's date, or be set in `data.json`?** | Product Owner | Auto-calculation from `DateTime.Now` is trivial but means screenshots change daily | | 3 | **How many months should the heatmap show?** The reference shows 4 (Jan–Apr). Should this be configurable? | Executives / PM | Affects grid layout (`repeat(N, 1fr)`) | | 4 | **Should the dashboard support multiple projects?** e.g., dropdown to switch between `project-a.json` and `project-b.json` | PM | Minor scope increase if yes | | 5 | **What browser will be used for screenshots?** Recommend Edge (Chromium) for consistency. | Developer | Affects CSS testing | | 6 | **Is the ADO Backlog link in the header functional or decorative?** | PM | If functional, it's just an `<a href>` tag—trivial | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed for local-only use. It reads project data from a `data.json` file and renders a timeline/milestone view with a heatmap grid showing shipped, in-progress, carryover, and blocker items. The design is intentionally simple—optimized for taking screenshots for PowerPoint decks.

**Primary recommendation:** Use a minimal Blazor Server app with zero external JavaScript dependencies. Render the SVG timeline natively in Razor components, use CSS Grid/Flexbox for the heatmap layout (matching the reference HTML exactly), and deserialize `data.json` with `System.Text.Json`. No database, no authentication, no cloud services. The entire solution should be a single `.sln` with one Blazor Server project, deployable via `dotnet run`.

---

## 2. Key Findings

- The reference `OriginalDesignConcept.html` is a static, self-contained HTML page with inline CSS and SVG—this translates almost 1:1 into Blazor Razor components with no framework overhead.
- The design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** extensively—both are natively supported in Blazor without any CSS framework dependency.
- The SVG timeline section is hand-authored markup—Blazor can render this directly in Razor with data binding, no charting library needed.
- The color palette is hardcoded and category-based (green=shipped, blue=in-progress, amber=carryover, red=blockers)—this maps cleanly to a simple enum-to-CSS-class mapping.
- The target resolution is 1920×1080 (fixed viewport for screenshots)—responsive design is unnecessary and should be avoided to maintain pixel-perfect screenshot fidelity.
- `System.Text.Json` in .NET 8 is mature and performant enough for deserializing a small config file; no need for Newtonsoft.Json.
- No authentication, no database, no external APIs—this dramatically simplifies the architecture to essentially a file-reading static renderer.
- Blazor Server's SignalR circuit is overkill for this use case, but it's the mandated stack and still works perfectly for local use with negligible overhead.
- Hot reload in .NET 8 Blazor Server works well, enabling fast iteration on layout and styling.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Mandated stack. Use Razor components, no JS interop needed. |
| **CSS Strategy** | Scoped CSS (`.razor.css`) + global `app.css` | Built-in | Use Blazor's CSS isolation for component styles. Global styles for the color palette and grid layout. |
| **CSS Layout** | CSS Grid + Flexbox | Native | Matches reference design exactly. No CSS framework (Bootstrap, Tailwind) needed or recommended. |
| **SVG Rendering** | Inline SVG in Razor | Native | Render timeline directly in `.razor` files with `@foreach` loops over milestone data. No charting library. |
| **Font** | Segoe UI | System font | Already on Windows. No web font loading needed. |
| **Icons** | None | — | Design uses CSS shapes (rotated squares for diamonds, circles). No icon library needed. |

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializer.Deserialize<T>()` with source generators for AOT-friendly serialization if desired. |
| **File Reading** | `System.IO.File.ReadAllTextAsync` | Built-in | Read `data.json` from a known path (e.g., `wwwroot/data/data.json` or app root). |
| **Configuration** | `IConfiguration` or direct file read | Built-in | For a simple app, direct file read is cleaner than wiring up `IConfiguration` for a single JSON file. |
| **Data Caching** | `IMemoryCache` or singleton service | `Microsoft.Extensions.Caching.Memory` (built-in) | Cache parsed data in memory. Reload on file change via `FileSystemWatcher` if desired. |

### Database

**None.** This project reads from a flat `data.json` file. No database is needed or recommended. If future requirements demand persistence (e.g., editing data through the UI), use **SQLite** via `Microsoft.Data.Sqlite` (3.x) or **LiteDB** (5.0.x) as an embedded document store—but this is explicitly out of scope.

### Testing

| Tool | Version | Purpose |
|------|---------|---------|
| **xUnit** | 2.7.x | Unit testing framework for .NET |
| **bUnit** | 1.28.x+ | Blazor component testing (renders components in-memory) |
| **FluentAssertions** | 6.12.x | Readable test assertions |
| **Moq** | 4.20.x | Mocking (if services are injected) |

Given the simplicity of this app, testing is low priority. If tests are written, focus on:
- JSON deserialization correctness (does `data.json` parse into the expected model?)
- SVG coordinate calculation logic (are milestones positioned correctly on the timeline?)

### Build & Tooling

| Tool | Version | Purpose |
|------|---------|---------|
| **.NET SDK** | 8.0.x LTS | Build, run, publish |
| **Visual Studio 2022** or **VS Code + C# Dev Kit** | Latest | IDE |
| **dotnet watch** | Built-in | Hot reload during development |

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Project Razor Component Tree

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── Models/
    │   ├── DashboardData.cs          // Root model
    │   ├── Milestone.cs              // Timeline milestones
    │   ├── HeatmapCategory.cs        // Shipped/InProgress/Carryover/Blockers
    │   └── HeatmapItem.cs            // Individual items in cells
    ├── Services/
    │   └── DashboardDataService.cs   // Reads and caches data.json
    ├── Components/
    │   ├── Layout/
    │   │   └── MainLayout.razor      // Full-page layout (no nav, no sidebar)
    │   ├── Pages/
    │   │   └── Dashboard.razor       // The single page
    │   ├── Header.razor              // Title bar + legend
    │   ├── Timeline.razor            // SVG milestone timeline
    │   ├── TimelineMilestone.razor   // Individual milestone marker
    │   ├── Heatmap.razor             // CSS Grid heatmap container
    │   ├── HeatmapRow.razor          // Single category row
    │   └── HeatmapCell.razor         // Single month×category cell
    ├── wwwroot/
    │   ├── css/
    │   │   └── app.css               // Global styles, color palette
    │   └── data/
    │       └── data.json             // Project data file
    └── Properties/
        └── launchSettings.json
```

### Key Design Decisions

**1. No component library.** MudBlazor, Radzen, and Syncfusion are all overkill. The design is custom CSS Grid + SVG. A component library would fight the design rather than help it.

**2. No JavaScript interop.** The entire design is achievable with pure Razor + CSS + inline SVG. Avoid `IJSRuntime` calls entirely.

**3. SVG timeline rendering in Razor.** The reference HTML hand-codes SVG elements. In Blazor, use a `Timeline.razor` component that:
- Accepts a list of `Milestone` objects
- Calculates X positions based on date-to-pixel mapping
- Renders `<line>`, `<circle>`, `<polygon>`, and `<text>` elements via `@foreach`
- Renders the "NOW" indicator line based on `DateTime.Now`

```csharp
// Example: date-to-X-position calculation
private double DateToX(DateTime date)
{
    var totalDays = (EndDate - StartDate).TotalDays;
    var elapsed = (date - StartDate).TotalDays;
    return (elapsed / totalDays) * SvgWidth;
}
```

**4. CSS class mapping by category.** Use an enum:
```csharp
public enum CategoryType { Shipped, InProgress, Carryover, Blockers }
```
Map to CSS classes: `ship-cell`, `prog-cell`, `carry-cell`, `block-cell`—directly matching the reference HTML's class names.

**5. Fixed 1920×1080 viewport.** Add to `app.css`:
```css
body { width: 1920px; height: 1080px; overflow: hidden; }
```
This ensures screenshot consistency. Do not add responsive breakpoints.

### Data Flow

```
data.json (file on disk)
    → DashboardDataService.LoadAsync()
        → System.Text.Json deserialization
            → DashboardData model (in-memory)
                → Injected into Dashboard.razor
                    → Passed as parameters to child components
                        → Rendered as HTML/SVG/CSS
```

### Data Model (data.json structure)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "organization": "Trusted Platform",
  "workstream": "Privacy Automation Workstream",
  "currentMonth": "April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "months": ["January", "February", "March", "April"],
  "currentMonthIndex": 3,
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      }
    ]
  },
  "heatmap": {
    "shipped": {
      "january": ["Item A", "Item B"],
      "february": ["Item C"],
      "march": ["Item D", "Item E"],
      "april": ["Item F"]
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

**None.** This is explicitly out of scope. The app runs locally, accessed via `https://localhost:5001` or `http://localhost:5000`. No login, no roles, no middleware.

If this ever needs to be shared on a network, the simplest addition would be a shared secret via a query string parameter or basic HTTP header check—but this is not recommended for the current scope.

### Data Protection

- `data.json` contains project status data, not PII or secrets. No encryption needed.
- If the JSON contains sensitive project names, ensure the machine running the app has appropriate access controls (standard OS-level file permissions).

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Self-contained single-file publish: `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` |
| **Hosting** | Kestrel (built into ASP.NET Core). No IIS, no reverse proxy, no Docker needed. |
| **Launch** | `dotnet run` or double-click the published `.exe` |
| **Port** | Configure in `launchSettings.json` or `appsettings.json`. Default `https://localhost:5001`. |
| **HTTPS** | Use the .NET dev cert (`dotnet dev-certs https --trust`) for local use. Optional—HTTP is fine for localhost screenshots. |

### Infrastructure Costs

**$0.** This runs on the developer's local machine. No cloud, no hosting, no CI/CD pipeline needed for MVP.

---

## 6. Risks & Trade-offs

### Low Risks (Manageable)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Blazor Server is heavier than needed** | Slight overhead from SignalR circuit for what is essentially a static page | Acceptable for local use. Alternative would be Blazor Static SSR (new in .NET 8), but Server mode is simpler to set up and the mandated stack. |
| **JSON schema drift** | If `data.json` structure changes, deserialization silently produces nulls | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and validate required fields on load. |
| **SVG coordinate math** | Timeline positioning bugs if date ranges change | Unit test the `DateToX()` mapping function. |
| **Browser rendering differences** | Screenshots may vary across browsers | Standardize on **Edge** (Chromium) for all screenshots. Document this. |

### Trade-offs Accepted

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No component library | Must hand-code all UI | Design is too custom for MudBlazor/Radzen to help. Hand-coding is faster here. |
| No database | Can't edit data through UI | Editing `data.json` in a text editor or VS Code is sufficient for the update cadence (monthly). |
| Fixed 1920×1080 | Not usable on mobile/tablets | Explicitly designed for screenshots, not interactive use. |
| No JS charting library | Must build SVG timeline manually | Chart.js or ApexCharts would require JS interop and wouldn't match the custom diamond/circle milestone markers from the design. |
| Blazor Server over Static SSR | Persistent SignalR connection for a read-only page | Server mode is simpler to configure and the team is familiar with it. Static SSR (RenderMode.Static) could be explored later. |

### Non-Risks (Explicitly Descoped)

- Scalability: Single user, local machine. Not a concern.
- Performance: Tiny JSON file, simple DOM. Not a concern.
- Availability: If it's down, run `dotnet run` again. Not a concern.
- Multi-tenancy: Single project dashboard. Not a concern.

---

## 7. Open Questions

| # | Question | Stakeholder | Impact |
|---|----------|-------------|--------|
| 1 | **How often will `data.json` be updated?** If monthly, manual editing is fine. If weekly+, consider a simple edit form in the UI. | Project Manager | Determines if CRUD UI is needed |
| 2 | **Should the "NOW" line auto-update to today's date, or be set in `data.json`?** | Product Owner | Auto-calculation from `DateTime.Now` is trivial but means screenshots change daily |
| 3 | **How many months should the heatmap show?** The reference shows 4 (Jan–Apr). Should this be configurable? | Executives / PM | Affects grid layout (`repeat(N, 1fr)`) |
| 4 | **Should the dashboard support multiple projects?** e.g., dropdown to switch between `project-a.json` and `project-b.json` | PM | Minor scope increase if yes |
| 5 | **What browser will be used for screenshots?** Recommend Edge (Chromium) for consistency. | Developer | Affects CSS testing |
| 6 | **Is the ADO Backlog link in the header functional or decorative?** | PM | If functional, it's just an `<a href>` tag—trivial |

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-perfect replica of `OriginalDesignConcept.html` as a Blazor Server app, reading from `data.json`.

1. **Scaffold the project** — `dotnet new blazorserver -n ReportingDashboard -f net8.0`
2. **Define the data model** — Create C# POCOs matching the `data.json` schema above.
3. **Create `data.json`** — Populate with fictional project data (e.g., "Project Phoenix" with milestones for a CRM migration).
4. **Build the `DashboardDataService`** — Read and deserialize `data.json` on startup, register as singleton.
5. **Port the CSS** — Copy the reference HTML's `<style>` block into `app.css` with minimal modifications (replace `body` fixed dimensions, keep all `.hm-*`, `.ship-*`, `.prog-*`, etc. classes).
6. **Build components top-down:**
   - `MainLayout.razor` — Remove default nav sidebar, use full-width layout
   - `Header.razor` — Title, subtitle, legend icons
   - `Timeline.razor` — SVG with data-bound milestone rendering
   - `Heatmap.razor` — CSS Grid container with category rows
7. **Test visually** — Open in Edge at 1920×1080, compare against reference design side-by-side.
8. **Take a screenshot** — Verify it looks presentation-ready.

### Phase 2: Polish (Optional, 1 day)

- Add `FileSystemWatcher` to auto-reload `data.json` when it changes (enables editing JSON while the app is running).
- Add subtle CSS transitions on hover for heatmap cells (optional flair for live demos).
- Add a print stylesheet (`@media print`) optimized for PDF export.
- Extract color palette into CSS custom properties (`--color-shipped: #34A853`) for easy theming.

### Phase 3: Multi-Project Support (Optional, if needed)

- Add a route parameter: `/dashboard/{projectName}`
- Load `{projectName}.json` from `wwwroot/data/`
- Add a simple project selector dropdown or landing page

### Quick Wins

1. **Fastest path to value:** Copy the reference HTML's CSS verbatim into `app.css`. This eliminates all design guesswork and ensures pixel-perfect fidelity from the start.
2. **Use `dotnet watch`:** Enables hot reload—change a Razor component, see it instantly in the browser. Critical for iterating on SVG positioning.
3. **Hardcode first, abstract later:** Get the page rendering with hardcoded data, then wire up `data.json`. Avoids debugging JSON parsing and layout issues simultaneously.

### NuGet Packages Required

```xml
<!-- ReportingDashboard.csproj — the only package references needed -->
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="8.0.*" />
<!-- That's it. No other packages needed for MVP. -->
```

Actually, for a standard Blazor Server project created with `dotnet new blazorserver`, **zero additional NuGet packages are required**. Everything needed (`System.Text.Json`, Kestrel, Razor components, CSS isolation) is included in the default `Microsoft.NET.Sdk.Web` SDK.

### Commands to Get Started

```powershell
# Create solution and project
dotnet new sln -n ReportingDashboard
dotnet new blazorserver -n ReportingDashboard -f net8.0
dotnet sln add ReportingDashboard/ReportingDashboard.csproj

# Run with hot reload
cd ReportingDashboard
dotnet watch run

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

---

## Appendix: Why NOT These Alternatives

| Technology | Why Not |
|-----------|---------|
| **MudBlazor / Radzen** | The design is fully custom. These libraries impose their own design language and would require overriding most defaults. More work, not less. |
| **Chart.js / ApexCharts** | Requires JS interop. The timeline SVG is simple enough to render natively. Custom milestone shapes (diamonds, circles) are easier in raw SVG than chart library customization. |
| **Blazor WebAssembly** | Requires downloading the .NET runtime to the browser. Slower startup. No benefit for a local-only, single-user app. |
| **Blazor Static SSR** | Viable alternative within .NET 8, but Server mode has better tooling support and the team is already familiar with it. |
| **Tailwind CSS** | Requires a build pipeline (PostCSS). The reference design already has complete CSS. Adding Tailwind would be overhead with no benefit. |
| **SQLite / LiteDB** | No persistence needed. JSON file is sufficient for read-only data that changes monthly. |
| **Docker** | Single-user local app. `dotnet run` is simpler than Docker for this use case. |

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
