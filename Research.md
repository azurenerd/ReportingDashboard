# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-11 17:35 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. It reads project data from a `data.json` file and renders a visual dashboard with a timeline/milestone section and a heatmap-style status grid (Shipped, In Progress, Carryover, Blockers). The dashboard is designed to be screenshotted for PowerPoint decks—meaning pixel-perfect rendering, clean typography, and a fixed 1920×1080 layout are critical. **Primary recommendation:** Keep the stack minimal. Use Blazor Server's built-in rendering with inline SVG for the timeline, pure CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for data loading. No charting libraries are needed—the design is simple enough to implement with raw SVG and CSS, which gives full control over the pixel-perfect output required. ---

### Key Findings

- The design is intentionally simple: a fixed-width (1920px) layout with a header, SVG timeline, and CSS Grid heatmap. This does not require a SPA framework's complexity—Blazor Server is sufficient as a rendering engine.
- The original HTML design uses **no JavaScript**, only CSS Grid, Flexbox, and inline SVG. This maps perfectly to Blazor's component model with zero JS interop needed.
- Data complexity is low: milestones (date, label, type), and categorized work items (shipped/in-progress/carryover/blockers) per time period. A flat JSON file is the right data source.
- The color palette and typography (Segoe UI) are Microsoft-standard, targeting executive audiences familiar with Microsoft design language.
- The "screenshot for PowerPoint" use case means: no animations, no hover states needed, no interactivity beyond loading data. Static rendering quality is the priority.
- Blazor Server's SignalR circuit is overkill for this use case but acceptable for a local-only tool. The alternative (Blazor Static SSR in .NET 8) could work but adds routing complexity without benefit for a single page.
- No authentication, no database, no API layer needed. This is a file-reader-to-HTML-renderer pipeline. ---
- Define the SVG viewBox as `0 0 1560 185` (matching original)
- Calculate X positions from dates: `x = (daysSinceStart / totalDays) * svgWidth`
- Render in Blazor using `@foreach` loops over milestone data
- Use `<defs>` for drop shadow filter (matching original's `<filter id="sh">`) This is ~80 lines of Blazor markup. A charting library would be harder to control. The heatmap uses a 5-column CSS Grid:
- Column 1: Row labels (160px fixed)
- Columns 2–5: Time period data cells (equal width via `1fr`) Each row is color-themed:
- **Shipped:** Green palette (`#E8F5E9`, `#F0FBF0`, `#D8F2DA`)
- **In Progress:** Blue palette (`#E3F2FD`, `#EEF4FE`, `#DAE8FB`)
- **Carryover:** Amber palette (`#FFF8E1`, `#FFFDE7`, `#FFF0B0`)
- **Blockers:** Red palette (`#FEF2F2`, `#FFF5F5`, `#FFE4E4`) The "current month" column gets a slightly darker shade (the `.apr` class in the original). **Implementation:** Define CSS custom properties per row category, use Blazor's `class` binding to apply the right category class per row.
```csharp
// Services/DashboardDataService.cs
public class DashboardDataService
{
    private readonly string _dataPath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.WebRootPath, "data.json");
    }

    public async Task<DashboardData> LoadAsync()
    {
        var json = await File.ReadAllTextAsync(_dataPath);
        return JsonSerializer.Deserialize<DashboardData>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
``` Register as scoped in `Program.cs`:
```csharp
builder.Services.AddScoped<DashboardDataService>();
``` ---
- **Scaffold project** — `dotnet new blazorserver-empty -n ReportingDashboard`
- **Define data model** — Create `DashboardData.cs` record types matching the JSON shape
- **Create sample `data.json`** — Fictional project with 3 milestone tracks, 4 months, ~5 items per status category
- **Build `Dashboard.razor`** — Single component with three sections:
- Header (title, subtitle, legend)
- Timeline (inline SVG with milestone rendering)
- Heatmap grid (CSS Grid with categorized items)
- **Apply CSS** — Port styles from `OriginalDesignConcept.html`, adapt to Blazor CSS isolation
- **Test at 1920×1080** — Verify screenshot quality
- **Improve the "NOW" indicator** — Auto-calculate position from `DateTime.Now`
- **Add data validation** — Graceful error display if `data.json` is malformed
- **Add `dotnet publish` profile** — Single-file self-contained executable
- **Document `data.json` schema** — README with field descriptions and example
- **Auto-screenshot script** — Playwright-based CLI to render PNG at 1920×1080
- **Multiple project configs** — Support `data.{project}.json` with a route parameter
- **Simple edit form** — Blazor form to update `data.json` without hand-editing
- **Dark mode** — Alternative color scheme for different presentation contexts
- **Reuse the original CSS verbatim** — The `OriginalDesignConcept.html` styles are clean and well-structured. Port them directly rather than rewriting.
- **Use Blazor CSS isolation** — `Dashboard.razor.css` scopes styles to the component automatically, preventing conflicts.
- **Use `record` types** — C# records give you immutability, value equality, and `with` expressions for free. Perfect for read-only dashboard data.
- **Use `@code` block, not code-behind** — For a single-page app, keep the C# logic in the `.razor` file. A code-behind class adds file management overhead with no benefit at this scale.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | Runtime | .NET 8 SDK | 8.0.x (LTS) | Long-term support through November 2026 | | Web Framework | Blazor Server | (included in .NET 8) | Interactive server-side rendering via SignalR | | Project Type | `blazorserver-empty` template | — | Minimal template, no sample pages | | Component | Technology | Notes | |-----------|-----------|-------| | Data Format | JSON (`data.json`) | Flat file, no database | | JSON Library | `System.Text.Json` | Built into .NET 8, no NuGet needed | | Data Models | C# record types | Immutable, concise, perfect for read-only config | | File Watching | `IFileSystemWatcher` or manual reload | Optional: hot-reload data without app restart | | Component | Technology | Notes | |-----------|-----------|-------| | Layout | CSS Grid + Flexbox | Matches original design exactly | | Timeline Visualization | Inline SVG via Blazor markup | No charting library needed; the timeline is simple lines, circles, diamonds | | Heatmap Grid | CSS Grid | 5-column grid: label + 4 time periods | | Icons/Shapes | SVG primitives | Diamonds (rotated squares), circles, lines—all in the original design | | Typography | Segoe UI (system font) | No web font loading needed on Windows | | CSS Architecture | Single scoped CSS file (`Dashboard.razor.css`) | Blazor CSS isolation, no preprocessor needed | | Library | Why Not | |---------|---------| | Radzen Blazor Charts | Overkill; adds 2MB+ of JS/CSS for simple shapes we can do in SVG | | MudBlazor Charts | Same; component library overhead for a single page | | ApexCharts.Blazor | JavaScript dependency; the design needs no interactive charts | | BlazorD3 | Unmaintained; D3 is overkill for this | **Rationale:** The original HTML design achieves its entire visualization with ~40 lines of SVG and CSS. Adding a charting library would increase complexity 10x for zero visual benefit and risk breaking the pixel-perfect layout.
```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── app.css          # Global styles (body, reset)
│       │   └── data.json            # Dashboard data
│       ├── Models/
│       │   └── DashboardData.cs     # Record types for JSON shape
│       ├── Services/
│       │   └── DashboardDataService.cs  # Reads & deserializes data.json
│       ├── Components/
│       │   ├── App.razor             # Root component
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Pages/
│       │       └── Dashboard.razor   # The single page
│       │       └── Dashboard.razor.css
│       └── appsettings.json
``` | Component | Technology | Version | Notes | |-----------|-----------|---------|-------| | Unit Tests | xUnit | 2.7+ | Test data loading and model mapping | | Blazor Component Tests | bUnit | 1.25+ | Optional; test component renders correct SVG/HTML | | Snapshot Testing | Verify | 23.x | Optional; catch unintended visual regressions in rendered HTML | | Tool | Purpose | |------|---------| | `dotnet watch` | Hot reload during development | | `dotnet publish` | Self-contained single-file publish for distribution | | Visual Studio 2022 / VS Code + C# Dev Kit | IDE | --- This is a **read-only, single-page, file-driven dashboard**. Do not over-architect.
```
data.json → DashboardDataService → Dashboard.razor → HTML/SVG/CSS → Browser → Screenshot
``` **No layers needed:** No repository pattern, no CQRS, no mediator. A single service class reads the JSON file and returns a strongly-typed model. The Blazor component renders it.
```csharp
// Models/DashboardData.cs
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimePeriod> Periods { get; init; }
    public StatusSection Shipped { get; init; }
    public StatusSection InProgress { get; init; }
    public StatusSection Carryover { get; init; }
    public StatusSection Blockers { get; init; }
}

public record ProjectInfo(string Title, string Subtitle, string BacklogUrl);
public record Milestone(string Id, string Label, string Date, string Type, string Track);
public record TimePeriod(string Label, bool IsCurrent);
public record StatusSection(List<StatusItem>[] ItemsByPeriod);
public record StatusItem(string Text);
``` The timeline in the original design is a horizontal SVG with:
- **Horizontal track lines** (colored per milestone track: blue, teal, gray)
- **Circle markers** for checkpoints
- **Diamond markers** for PoC milestones (rotated squares via `<polygon>`)
- **Diamond markers** (green) for production releases
- **Vertical dashed line** for "NOW" indicator
- **Month labels** evenly spaced

### Considerations & Risks

- | Concern | Approach | |---------|----------| | Authentication | None. Local-only tool, single user. | | Authorization | None. | | HTTPS | Not required for localhost; `dotnet run` serves HTTP by default | | Input Validation | Minimal; validate JSON schema on load to prevent rendering errors | | CORS | Not applicable; no API consumers | | Content Security Policy | Optional; add if concerned about local network exposure | | Scenario | Approach | |----------|----------| | Development | `dotnet watch run` from project directory | | Distribution | `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` | | Port Config | Default `https://localhost:5001` or configure in `launchSettings.json` | | Data Updates | Edit `data.json` in `wwwroot/`, refresh browser | **Self-contained publish** produces a single `.exe` (~70MB) that runs on any Windows machine without .NET installed. Ideal for sharing with colleagues who want to run their own dashboard. Since the primary output is PowerPoint screenshots:
- Open dashboard at `localhost:5001`
- Browser window at 1920×1080 (or use Chrome DevTools device toolbar)
- Screenshot with Win+Shift+S or browser extension
- Paste into PowerPoint **Optional enhancement:** Add a `dotnet tool` or Playwright script to auto-capture screenshots:
```bash
# Optional: automated screenshot via Playwright
dotnet tool install --global Microsoft.Playwright.CLI
playwright screenshot http://localhost:5001 dashboard.png --viewport-size="1920,1080"
``` --- **Impact:** Low **Details:** Blazor Server maintains a SignalR WebSocket connection per client, which is overkill for a page that could be static HTML. However, for a local single-user tool, this overhead is negligible (~2MB RAM). **Mitigation:** If this ever becomes a concern, .NET 8's Static SSR mode (`@rendermode="Static"`) can render the page without SignalR. This is a one-line change. **Impact:** Medium **Details:** SVG drop shadows and text positioning vary slightly between Chrome, Edge, and Firefox. **Mitigation:** Target a single browser (Edge/Chrome, since this is a Microsoft shop). Test at exactly 1920×1080. Use `font-family: 'Segoe UI'` which renders consistently on Windows. **Impact:** Low **Details:** As the dashboard evolves, `data.json` may gain fields that don't match the C# model. **Mitigation:** Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and make all model properties nullable or provide defaults. Add a JSON schema file (`data.schema.json`) for documentation. **Impact:** High (probability) **Details:** The biggest risk is making this more complex than it needs to be. This is a JSON-to-HTML renderer. Every abstraction layer (repositories, interfaces, dependency injection beyond basics, MediatR, etc.) adds cognitive load without value. **Mitigation:** Enforce the rule: if a component has only one implementation, don't create an interface for it. If logic fits in 20 lines, don't create a service for it. **Decision:** Acceptable. Executives view monthly snapshots. The data update workflow is: edit JSON → refresh browser → screenshot. Real-time data feeds would add significant complexity for no user value. **Decision:** Acceptable for MVP. Browser screenshot is sufficient. If needed later, add a `/print` route with `@media print` CSS or use Playwright for automated PDF generation. ---
- **Data authoring workflow:** Will the team edit `data.json` by hand, or should we build a simple editor form? (Recommendation: start with hand-editing; add a form only if multiple people update it frequently.)
- **Multi-project support:** Should the dashboard support switching between projects, or is it one dashboard per project instance? (Recommendation: one instance per project; keep it simple.)
- **Historical snapshots:** Should previous months' data be preserved for comparison? (Recommendation: use Git versioning of `data.json` rather than building history into the app.)
- **Color customization:** Should project teams be able to customize the color palette per category? (Recommendation: defer; hardcode the Microsoft-standard palette from the original design.)
- **Timeline date range:** Should the timeline auto-calculate its range from milestone dates, or use a fixed range defined in `data.json`? (Recommendation: define `startDate` and `endDate` in JSON for explicit control over the viewport.) ---

### Detailed Analysis

# Research: Executive Reporting Dashboard — Technology Stack & Architecture

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. It reads project data from a `data.json` file and renders a visual dashboard with a timeline/milestone section and a heatmap-style status grid (Shipped, In Progress, Carryover, Blockers). The dashboard is designed to be screenshotted for PowerPoint decks—meaning pixel-perfect rendering, clean typography, and a fixed 1920×1080 layout are critical.

**Primary recommendation:** Keep the stack minimal. Use Blazor Server's built-in rendering with inline SVG for the timeline, pure CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for data loading. No charting libraries are needed—the design is simple enough to implement with raw SVG and CSS, which gives full control over the pixel-perfect output required.

---

## 2. Key Findings

- The design is intentionally simple: a fixed-width (1920px) layout with a header, SVG timeline, and CSS Grid heatmap. This does not require a SPA framework's complexity—Blazor Server is sufficient as a rendering engine.
- The original HTML design uses **no JavaScript**, only CSS Grid, Flexbox, and inline SVG. This maps perfectly to Blazor's component model with zero JS interop needed.
- Data complexity is low: milestones (date, label, type), and categorized work items (shipped/in-progress/carryover/blockers) per time period. A flat JSON file is the right data source.
- The color palette and typography (Segoe UI) are Microsoft-standard, targeting executive audiences familiar with Microsoft design language.
- The "screenshot for PowerPoint" use case means: no animations, no hover states needed, no interactivity beyond loading data. Static rendering quality is the priority.
- Blazor Server's SignalR circuit is overkill for this use case but acceptable for a local-only tool. The alternative (Blazor Static SSR in .NET 8) could work but adds routing complexity without benefit for a single page.
- No authentication, no database, no API layer needed. This is a file-reader-to-HTML-renderer pipeline.

---

## 3. Recommended Technology Stack

### Runtime & Framework

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| Runtime | .NET 8 SDK | 8.0.x (LTS) | Long-term support through November 2026 |
| Web Framework | Blazor Server | (included in .NET 8) | Interactive server-side rendering via SignalR |
| Project Type | `blazorserver-empty` template | — | Minimal template, no sample pages |

### Data Layer

| Component | Technology | Notes |
|-----------|-----------|-------|
| Data Format | JSON (`data.json`) | Flat file, no database |
| JSON Library | `System.Text.Json` | Built into .NET 8, no NuGet needed |
| Data Models | C# record types | Immutable, concise, perfect for read-only config |
| File Watching | `IFileSystemWatcher` or manual reload | Optional: hot-reload data without app restart |

### Frontend / UI

| Component | Technology | Notes |
|-----------|-----------|-------|
| Layout | CSS Grid + Flexbox | Matches original design exactly |
| Timeline Visualization | Inline SVG via Blazor markup | No charting library needed; the timeline is simple lines, circles, diamonds |
| Heatmap Grid | CSS Grid | 5-column grid: label + 4 time periods |
| Icons/Shapes | SVG primitives | Diamonds (rotated squares), circles, lines—all in the original design |
| Typography | Segoe UI (system font) | No web font loading needed on Windows |
| CSS Architecture | Single scoped CSS file (`Dashboard.razor.css`) | Blazor CSS isolation, no preprocessor needed |

### Charting Libraries — NOT Recommended

| Library | Why Not |
|---------|---------|
| Radzen Blazor Charts | Overkill; adds 2MB+ of JS/CSS for simple shapes we can do in SVG |
| MudBlazor Charts | Same; component library overhead for a single page |
| ApexCharts.Blazor | JavaScript dependency; the design needs no interactive charts |
| BlazorD3 | Unmaintained; D3 is overkill for this |

**Rationale:** The original HTML design achieves its entire visualization with ~40 lines of SVG and CSS. Adding a charting library would increase complexity 10x for zero visual benefit and risk breaking the pixel-perfect layout.

### Project Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── app.css          # Global styles (body, reset)
│       │   └── data.json            # Dashboard data
│       ├── Models/
│       │   └── DashboardData.cs     # Record types for JSON shape
│       ├── Services/
│       │   └── DashboardDataService.cs  # Reads & deserializes data.json
│       ├── Components/
│       │   ├── App.razor             # Root component
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Pages/
│       │       └── Dashboard.razor   # The single page
│       │       └── Dashboard.razor.css
│       └── appsettings.json
```

### Testing (Lightweight)

| Component | Technology | Version | Notes |
|-----------|-----------|---------|-------|
| Unit Tests | xUnit | 2.7+ | Test data loading and model mapping |
| Blazor Component Tests | bUnit | 1.25+ | Optional; test component renders correct SVG/HTML |
| Snapshot Testing | Verify | 23.x | Optional; catch unintended visual regressions in rendered HTML |

### Build & Dev Tooling

| Tool | Purpose |
|------|---------|
| `dotnet watch` | Hot reload during development |
| `dotnet publish` | Self-contained single-file publish for distribution |
| Visual Studio 2022 / VS Code + C# Dev Kit | IDE |

---

## 4. Architecture Recommendations

### Pattern: Minimal Service Architecture

This is a **read-only, single-page, file-driven dashboard**. Do not over-architect.

```
data.json → DashboardDataService → Dashboard.razor → HTML/SVG/CSS → Browser → Screenshot
```

**No layers needed:** No repository pattern, no CQRS, no mediator. A single service class reads the JSON file and returns a strongly-typed model. The Blazor component renders it.

### Data Model Design

```csharp
// Models/DashboardData.cs
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimePeriod> Periods { get; init; }
    public StatusSection Shipped { get; init; }
    public StatusSection InProgress { get; init; }
    public StatusSection Carryover { get; init; }
    public StatusSection Blockers { get; init; }
}

public record ProjectInfo(string Title, string Subtitle, string BacklogUrl);
public record Milestone(string Id, string Label, string Date, string Type, string Track);
public record TimePeriod(string Label, bool IsCurrent);
public record StatusSection(List<StatusItem>[] ItemsByPeriod);
public record StatusItem(string Text);
```

### SVG Timeline Rendering Strategy

The timeline in the original design is a horizontal SVG with:
- **Horizontal track lines** (colored per milestone track: blue, teal, gray)
- **Circle markers** for checkpoints
- **Diamond markers** for PoC milestones (rotated squares via `<polygon>`)
- **Diamond markers** (green) for production releases
- **Vertical dashed line** for "NOW" indicator
- **Month labels** evenly spaced

**Implementation approach:**
1. Define the SVG viewBox as `0 0 1560 185` (matching original)
2. Calculate X positions from dates: `x = (daysSinceStart / totalDays) * svgWidth`
3. Render in Blazor using `@foreach` loops over milestone data
4. Use `<defs>` for drop shadow filter (matching original's `<filter id="sh">`)

This is ~80 lines of Blazor markup. A charting library would be harder to control.

### CSS Grid Heatmap Strategy

The heatmap uses a 5-column CSS Grid:
- Column 1: Row labels (160px fixed)
- Columns 2–5: Time period data cells (equal width via `1fr`)

Each row is color-themed:
- **Shipped:** Green palette (`#E8F5E9`, `#F0FBF0`, `#D8F2DA`)
- **In Progress:** Blue palette (`#E3F2FD`, `#EEF4FE`, `#DAE8FB`)
- **Carryover:** Amber palette (`#FFF8E1`, `#FFFDE7`, `#FFF0B0`)
- **Blockers:** Red palette (`#FEF2F2`, `#FFF5F5`, `#FFE4E4`)

The "current month" column gets a slightly darker shade (the `.apr` class in the original).

**Implementation:** Define CSS custom properties per row category, use Blazor's `class` binding to apply the right category class per row.

### Data Loading Strategy

```csharp
// Services/DashboardDataService.cs
public class DashboardDataService
{
    private readonly string _dataPath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.WebRootPath, "data.json");
    }

    public async Task<DashboardData> LoadAsync()
    {
        var json = await File.ReadAllTextAsync(_dataPath);
        return JsonSerializer.Deserialize<DashboardData>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
```

Register as scoped in `Program.cs`:
```csharp
builder.Services.AddScoped<DashboardDataService>();
```

---

## 5. Security & Infrastructure

### Security Posture: Minimal (By Design)

| Concern | Approach |
|---------|----------|
| Authentication | None. Local-only tool, single user. |
| Authorization | None. |
| HTTPS | Not required for localhost; `dotnet run` serves HTTP by default |
| Input Validation | Minimal; validate JSON schema on load to prevent rendering errors |
| CORS | Not applicable; no API consumers |
| Content Security Policy | Optional; add if concerned about local network exposure |

### Hosting & Deployment

| Scenario | Approach |
|----------|----------|
| Development | `dotnet watch run` from project directory |
| Distribution | `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` |
| Port Config | Default `https://localhost:5001` or configure in `launchSettings.json` |
| Data Updates | Edit `data.json` in `wwwroot/`, refresh browser |

**Self-contained publish** produces a single `.exe` (~70MB) that runs on any Windows machine without .NET installed. Ideal for sharing with colleagues who want to run their own dashboard.

### Screenshot Workflow

Since the primary output is PowerPoint screenshots:
1. Open dashboard at `localhost:5001`
2. Browser window at 1920×1080 (or use Chrome DevTools device toolbar)
3. Screenshot with Win+Shift+S or browser extension
4. Paste into PowerPoint

**Optional enhancement:** Add a `dotnet tool` or Playwright script to auto-capture screenshots:
```bash
# Optional: automated screenshot via Playwright
dotnet tool install --global Microsoft.Playwright.CLI
playwright screenshot http://localhost:5001 dashboard.png --viewport-size="1920,1080"
```

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Overhead for a Static Page

**Impact:** Low
**Details:** Blazor Server maintains a SignalR WebSocket connection per client, which is overkill for a page that could be static HTML. However, for a local single-user tool, this overhead is negligible (~2MB RAM).
**Mitigation:** If this ever becomes a concern, .NET 8's Static SSR mode (`@rendermode="Static"`) can render the page without SignalR. This is a one-line change.

### Risk: SVG Rendering Inconsistencies Across Browsers

**Impact:** Medium
**Details:** SVG drop shadows and text positioning vary slightly between Chrome, Edge, and Firefox.
**Mitigation:** Target a single browser (Edge/Chrome, since this is a Microsoft shop). Test at exactly 1920×1080. Use `font-family: 'Segoe UI'` which renders consistently on Windows.

### Risk: JSON Schema Drift

**Impact:** Low
**Details:** As the dashboard evolves, `data.json` may gain fields that don't match the C# model.
**Mitigation:** Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and make all model properties nullable or provide defaults. Add a JSON schema file (`data.schema.json`) for documentation.

### Risk: Over-Engineering

**Impact:** High (probability)
**Details:** The biggest risk is making this more complex than it needs to be. This is a JSON-to-HTML renderer. Every abstraction layer (repositories, interfaces, dependency injection beyond basics, MediatR, etc.) adds cognitive load without value.
**Mitigation:** Enforce the rule: if a component has only one implementation, don't create an interface for it. If logic fits in 20 lines, don't create a service for it.

### Trade-off: No Real-Time Data

**Decision:** Acceptable. Executives view monthly snapshots. The data update workflow is: edit JSON → refresh browser → screenshot. Real-time data feeds would add significant complexity for no user value.

### Trade-off: No Print/Export Feature

**Decision:** Acceptable for MVP. Browser screenshot is sufficient. If needed later, add a `/print` route with `@media print` CSS or use Playwright for automated PDF generation.

---

## 7. Open Questions

1. **Data authoring workflow:** Will the team edit `data.json` by hand, or should we build a simple editor form? (Recommendation: start with hand-editing; add a form only if multiple people update it frequently.)

2. **Multi-project support:** Should the dashboard support switching between projects, or is it one dashboard per project instance? (Recommendation: one instance per project; keep it simple.)

3. **Historical snapshots:** Should previous months' data be preserved for comparison? (Recommendation: use Git versioning of `data.json` rather than building history into the app.)

4. **Color customization:** Should project teams be able to customize the color palette per category? (Recommendation: defer; hardcode the Microsoft-standard palette from the original design.)

5. **Timeline date range:** Should the timeline auto-calculate its range from milestone dates, or use a fixed range defined in `data.json`? (Recommendation: define `startDate` and `endDate` in JSON for explicit control over the viewport.)

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

1. **Scaffold project** — `dotnet new blazorserver-empty -n ReportingDashboard`
2. **Define data model** — Create `DashboardData.cs` record types matching the JSON shape
3. **Create sample `data.json`** — Fictional project with 3 milestone tracks, 4 months, ~5 items per status category
4. **Build `Dashboard.razor`** — Single component with three sections:
   - Header (title, subtitle, legend)
   - Timeline (inline SVG with milestone rendering)
   - Heatmap grid (CSS Grid with categorized items)
5. **Apply CSS** — Port styles from `OriginalDesignConcept.html`, adapt to Blazor CSS isolation
6. **Test at 1920×1080** — Verify screenshot quality

### Phase 2: Polish (1 day)

1. **Improve the "NOW" indicator** — Auto-calculate position from `DateTime.Now`
2. **Add data validation** — Graceful error display if `data.json` is malformed
3. **Add `dotnet publish` profile** — Single-file self-contained executable
4. **Document `data.json` schema** — README with field descriptions and example

### Phase 3: Optional Enhancements (as needed)

1. **Auto-screenshot script** — Playwright-based CLI to render PNG at 1920×1080
2. **Multiple project configs** — Support `data.{project}.json` with a route parameter
3. **Simple edit form** — Blazor form to update `data.json` without hand-editing
4. **Dark mode** — Alternative color scheme for different presentation contexts

### Quick Wins

- **Reuse the original CSS verbatim** — The `OriginalDesignConcept.html` styles are clean and well-structured. Port them directly rather than rewriting.
- **Use Blazor CSS isolation** — `Dashboard.razor.css` scopes styles to the component automatically, preventing conflicts.
- **Use `record` types** — C# records give you immutability, value equality, and `with` expressions for free. Perfect for read-only dashboard data.
- **Use `@code` block, not code-behind** — For a single-page app, keep the C# logic in the `.razor` file. A code-behind class adds file management overhead with no benefit at this scale.

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
