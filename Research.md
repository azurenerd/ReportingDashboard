# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 23:21 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and deliverable tracking. The dashboard reads from a local `data.json` configuration file and renders a screenshot-friendly view optimized for PowerPoint decks. The architecture is intentionally simple: no authentication, no cloud services, no database server—just a local Blazor Server app serving a single responsive page. ---

### Key Findings

- The reference design (`OriginalDesignConcept.html`) uses pure HTML/CSS/SVG with no JavaScript frameworks, making it trivially portable to Blazor Razor components.
- CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`) and Flexbox are the only layout techniques required—no third-party CSS framework needed.
- The timeline/Gantt visualization is built with inline SVG elements (lines, circles, polygons, text), which Blazor can render natively in `.razor` files with C# loop logic.
- The heatmap is a styled HTML grid, not a canvas or charting library artifact—no charting dependency is required.
- A local `data.json` file is sufficient for all data needs; no database is warranted for this scope.
- Blazor Server's SignalR circuit is irrelevant for this use case (static screenshot pages), but adds zero complexity to leave in place.
- The fixed 1920×1080 viewport in the reference design is intentional for screenshot fidelity—responsive design is a non-goal.
- .NET 8 is LTS (supported through November 2026), making it the correct target for stability.
- ---
- `dotnet new blazorserver -n ReportingDashboard -f net8.0`
- Create `data.json` with fictional project data matching the reference design's structure.
- Create `DashboardData.cs` with C# record types matching the JSON schema.
- Create `DashboardDataService.cs` that reads and deserializes the JSON file.
- Replace the default `Home.razor` with `Dashboard.razor`, porting the reference HTML structure into Razor with `@foreach` loops over the data.
- Port the reference CSS into `Dashboard.razor.css` as scoped styles.
- Verify visual parity with the reference design at 1920×1080.
- Dynamic "Now" line calculated from system date.
- Dynamic month columns driven by JSON (not hardcoded to 4).
- Current-month highlight applied automatically.
- Empty cell graceful handling.
- Tooltip or subtle metadata (e.g., item count per cell).
- Print stylesheet for clean PDF export.
- `FileSystemWatcher` for live reload when `data.json` changes.
- Error page for malformed JSON with helpful message.
- README with screenshot procedure documentation.
- **Fastest path to value:** Skip Phase 2 and 3 entirely. Phase 1 alone produces a functional, screenshot-ready dashboard. The reference design is already good—data-driving it is the only essential improvement.
- **Copy-paste the CSS:** Don't rewrite the reference design's styles. Copy them verbatim into the scoped CSS file, then parameterize only what needs to be dynamic (month count, current month class).
- **Use records everywhere:** `record Milestone(string Label, string Date, string Type)` is one line of code and gives you immutability, value equality, and `ToString()` for free.
- `Microsoft.AspNetCore.App` (implicit framework reference)
- `System.Text.Json` (included in framework)
- No additional NuGet packages are needed. This is the simplest possible dependency footprint.
- ```bash
- dotnet new sln -n ReportingDashboard
- dotnet new blazorserver -n ReportingDashboard -f net8.0 -o src/ReportingDashboard
- dotnet sln add src/ReportingDashboard/ReportingDashboard.csproj
- dotnet build
- dotnet run --project src/ReportingDashboard
- dotnet publish src/ReportingDashboard -c Release -r win-x64 --self-contained -o ./publish
- ```

### Recommended Tools & Technologies

- | Component | Recommendation | Version | Notes |
- |-----------|---------------|---------|-------|
- | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional packages. Single `.razor` page. |
- | **CSS Approach** | Scoped CSS (`Dashboard.razor.css`) | Built-in | Mirrors the reference design's flat CSS. No Tailwind/Bootstrap needed. |
- | **Layout** | CSS Grid + Flexbox | Native | Exact grid from reference: `160px repeat(N,1fr)` columns, 4 status rows. |
- | **Timeline/Gantt** | Inline SVG in Razor | Native | Use `<svg>` with C# `@foreach` loops to render milestones, date lines, markers. |
- | **Heatmap** | Styled `<div>` grid | Native | Color-coded cells with CSS classes per status category. |
- | **Icons/Markers** | SVG shapes (polygons, circles) | Native | Diamond = PoC milestone, circle = checkpoint, per reference design. |
- | **Font** | Segoe UI, Arial fallback | System | No web font loading needed—Segoe UI is available on all Windows machines where this runs. |
- | Component | Recommendation | Version | Notes |
- |-----------|---------------|---------|-------|
- | **Runtime** | .NET 8.0 LTS | 8.0.x | `dotnet new blazorserver` template. |
- | **JSON Deserialization** | `System.Text.Json` | Built-in (.NET 8) | No Newtonsoft needed. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. |
- | **Configuration** | `IConfiguration` + custom JSON | Built-in | Load `data.json` via `builder.Configuration.AddJsonFile("data.json")` or manual `File.ReadAllText`. |
- | **Data Models** | C# records | Built-in | Immutable POCOs: `record Milestone(string Name, DateOnly Date, string Type)`, etc. |
- | **File Watching** | `FileSystemWatcher` | Built-in | Optional: hot-reload dashboard when `data.json` changes. |
- ```
- ReportingDashboard/
- ├── ReportingDashboard.sln
- ├── src/
- │   └── ReportingDashboard/
- │       ├── ReportingDashboard.csproj
- │       ├── Program.cs
- │       ├── Data/
- │       │   ├── data.json                    # The config file
- │       │   ├── DashboardData.cs             # C# record models
- │       │   └── DashboardDataService.cs      # Loads & deserializes JSON
- │       ├── Components/
- │       │   └── Pages/
- │       │       ├── Dashboard.razor           # The single page
- │       │       └── Dashboard.razor.css       # Scoped styles from reference
- │       └── wwwroot/
- │           └── css/
- │               └── app.css                   # Minimal global resets
- ```
- | Component | Recommendation | Version | Notes |
- |-----------|---------------|---------|-------|
- | **Unit Testing** | xUnit | 2.7+ | Test JSON deserialization, date calculations, status categorization. |
- | **Blazor Component Testing** | bUnit | 1.25+ | Optional. Test that Razor components render correct SVG/HTML for given data. |
- | **Assertions** | FluentAssertions | 6.12+ | Readable test assertions. |
- | Component | Recommendation | Notes |
- |-----------|---------------|-------|
- | **Build** | `dotnet build` | Standard .NET CLI. |
- | **Run** | `dotnet run` | Launches Kestrel on `https://localhost:5001`. |
- | **Publish** | `dotnet publish -c Release` | Self-contained optional: `--self-contained -r win-x64`. |
- ---
- This project does **not** need Clean Architecture, CQRS, MediatR, or repository patterns. It is a single-page read-only dashboard. The appropriate architecture is:
- ```
- data.json → DashboardDataService → Dashboard.razor → HTML/CSS/SVG output
- ```
- Three layers, three files of logic. Anything more is over-engineering.
- **Startup:** `DashboardDataService` reads `data.json` from disk and deserializes into `DashboardData` record.
- **Request:** Blazor Server renders `Dashboard.razor`, injecting `DashboardDataService` via DI.
- **Render:** Razor component iterates over milestones, status items, and months to produce the grid and SVG timeline.
- **Refresh:** User refreshes browser (or optionally, `FileSystemWatcher` triggers `StateHasChanged()`).
- ```json
- {
- "title": "Project Phoenix Release Roadmap",
- "organization": "Engineering Platform • Core Services Workstream",
- "reportMonth": "April 2026",
- "timelineStart": "2026-01-01",
- "timelineEnd": "2026-06-30",
- "milestoneStreams": [
- {
- "id": "M1",
- "name": "API Gateway & Auth",
- "color": "#0078D4",
- "milestones": [
- { "date": "2026-01-15", "label": "Kickoff", "type": "checkpoint" },
- { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "poc" },
- { "date": "2026-05-01", "label": "May Prod", "type": "production" }
- ]
- }
- ],
- "months": ["January", "February", "March", "April"],
- "currentMonth": "April",
- "statusRows": [
- {
- "category": "shipped",
- "label": "Shipped",
- "items": {
- "January": ["OAuth2 flow implemented", "API rate limiting"],
- "February": ["Token refresh logic"],
- "March": ["Load test harness"],
- "April": ["Prod deployment scripts"]
- }
- },
- {
- "category": "in-progress",
- "label": "In Progress",
- "items": { ... }
- },
- {
- "category": "carryover",
- "label": "Carryover",
- "items": { ... }
- },
- {
- "category": "blockers",
- "label": "Blockers",
- "items": { ... }
- }
- ]
- }
- ```
- The reference design maps dates to x-coordinates on a 1560px-wide SVG. The C# logic:
- ```csharp
- double DateToX(DateOnly date, DateOnly start, DateOnly end, double width)
- => (date.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber) * width;
- ```
- Milestone types map to SVG shapes:
- **Checkpoint** → `<circle>` (small, gray fill)
- **PoC Milestone** → `<polygon>` diamond (gold `#F4B400`)
- **Production Release** → `<polygon>` diamond (green `#34A853`)
- **"Now" line** → `<line>` dashed red (`#EA4335`)
- Port the reference CSS directly into `Dashboard.razor.css` as scoped styles. Key decisions:
- **Fixed viewport:** Keep `width: 1920px; height: 1080px; overflow: hidden` on the body for screenshot fidelity.
- **Color system:** Define CSS custom properties for the four status categories:
- ```css
- :root {
- --shipped-bg: #F0FBF0; --shipped-accent: #34A853; --shipped-header: #E8F5E9;
- --progress-bg: #EEF4FE; --progress-accent: #0078D4; --progress-header: #E3F2FD;
- --carry-bg: #FFFDE7; --carry-accent: #F4B400; --carry-header: #FFF8E1;
- --block-bg: #FFF5F5; --block-accent: #EA4335; --block-header: #FEF2F2;
- }
- ```
- **Current month highlight:** The `apr` class adds a slightly darker background. Generalize this to `.current-month` applied dynamically based on `data.json`.
- The reference HTML is static. Data-driven Blazor improvements:
- **Dynamic column count:** Months array in JSON drives grid columns, not hardcoded to 4.
- **Auto "Now" line positioning:** Calculated from `DateTime.Now` vs. timeline range.
- **Empty state handling:** If a status category has no items for a month, render a subtle "—" instead of blank.
- **Overflow truncation:** Long item text gets `text-overflow: ellipsis` to prevent cell blowout.
- **Print-friendly:** Add `@media print` styles that hide browser chrome for direct-to-PDF screenshot alternative.
- ---

### Considerations & Risks

- `data.json` contains no PII or secrets—it's project status information.
- No encryption needed for data at rest or in transit (localhost only).
- If `data.json` is checked into a repo, ensure no sensitive content leaks (reviewer names, internal codenames that are confidential).
- | Aspect | Recommendation |
- |--------|---------------|
- | **Runtime** | Local Kestrel server via `dotnet run` |
- | **Port** | `https://localhost:5001` (default) or configure in `launchSettings.json` |
- | **Deployment** | `dotnet publish -c Release -o ./publish` → run `ReportingDashboard.exe` |
- | **Self-Contained** | Optional: `dotnet publish -r win-x64 --self-contained` for machines without .NET runtime |
- | **Docker** | Not needed. This is a local tool. |
- | **CI/CD** | Not needed. Manual build-and-run. |
- ---
- The design uses `Segoe UI` font and a fixed 1920×1080 viewport. Screenshots taken on machines with different DPI scaling, browser zoom levels, or missing Segoe UI font will look different.
- Blazor Server maintains a SignalR WebSocket connection for interactivity. This page has zero interactivity—it's read-only. The SignalR circuit adds ~50KB of JS and a persistent connection.
- As the dashboard evolves (new status categories, additional timeline streams), the `data.json` schema will change. Without validation, malformed JSON will produce runtime exceptions.
- The biggest risk to this project is adding unnecessary complexity: auth, databases, real-time updates, component libraries, multi-page navigation, build pipelines, Docker containers. The user explicitly wants "super simple" and "screenshot for PowerPoint."
- ---
- **How many milestone streams?** The reference has 3 (M1, M2, M3). Should `data.json` support arbitrary N, or is 3 the fixed maximum? (Recommendation: support N, but test with 3–5.)
- **How many months displayed?** Reference shows 4 columns (Jan–Apr). Should this always show the last 4 months, or is it configurable? (Recommendation: configurable in `data.json`.)
- **Who edits `data.json`?** If the user edits it manually, keep the schema flat and simple. If a future tool generates it, the schema can be more structured. (Recommendation: assume manual editing—keep it human-friendly.)
- **Multiple projects?** Will there ever be multiple dashboards for different projects? If so, support multiple JSON files (`project-phoenix.json`, `project-atlas.json`) selected via URL route. (Recommendation: defer—start with single `data.json`.)
- **Screenshot automation?** Should the app include a "download as PNG" button? This would require Playwright or a headless browser dependency. (Recommendation: defer—use browser screenshot or Snipping Tool for now.)
- ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and deliverable tracking. The dashboard reads from a local `data.json` configuration file and renders a screenshot-friendly view optimized for PowerPoint decks. The architecture is intentionally simple: no authentication, no cloud services, no database server—just a local Blazor Server app serving a single responsive page.

**Primary Recommendation:** Use a minimal Blazor Server project with inline SVG generation for the timeline, CSS Grid for the heatmap layout, and `System.Text.Json` for config deserialization. No charting library is needed—the design is achievable with pure HTML/CSS/SVG rendered server-side through Razor components. Target a single afternoon to reach functional parity with the HTML reference design, then iterate on data-driven improvements.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) uses pure HTML/CSS/SVG with no JavaScript frameworks, making it trivially portable to Blazor Razor components.
- CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`) and Flexbox are the only layout techniques required—no third-party CSS framework needed.
- The timeline/Gantt visualization is built with inline SVG elements (lines, circles, polygons, text), which Blazor can render natively in `.razor` files with C# loop logic.
- The heatmap is a styled HTML grid, not a canvas or charting library artifact—no charting dependency is required.
- A local `data.json` file is sufficient for all data needs; no database is warranted for this scope.
- Blazor Server's SignalR circuit is irrelevant for this use case (static screenshot pages), but adds zero complexity to leave in place.
- The fixed 1920×1080 viewport in the reference design is intentional for screenshot fidelity—responsive design is a non-goal.
- .NET 8 is LTS (supported through November 2026), making it the correct target for stability.

---

## 3. Recommended Technology Stack

### Frontend / UI Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional packages. Single `.razor` page. |
| **CSS Approach** | Scoped CSS (`Dashboard.razor.css`) | Built-in | Mirrors the reference design's flat CSS. No Tailwind/Bootstrap needed. |
| **Layout** | CSS Grid + Flexbox | Native | Exact grid from reference: `160px repeat(N,1fr)` columns, 4 status rows. |
| **Timeline/Gantt** | Inline SVG in Razor | Native | Use `<svg>` with C# `@foreach` loops to render milestones, date lines, markers. |
| **Heatmap** | Styled `<div>` grid | Native | Color-coded cells with CSS classes per status category. |
| **Icons/Markers** | SVG shapes (polygons, circles) | Native | Diamond = PoC milestone, circle = checkpoint, per reference design. |
| **Font** | Segoe UI, Arial fallback | System | No web font loading needed—Segoe UI is available on all Windows machines where this runs. |

**Why no charting library?** Libraries like `Radzen.Blazor`, `MudBlazor.Charts`, or `Blazorise.Charts` add 200KB+ of JS interop, configuration complexity, and render charts that look nothing like the clean, custom reference design. The reference design's SVG is ~50 lines of markup—generating it in Razor is simpler than configuring any charting API.

### Backend / Data Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | .NET 8.0 LTS | 8.0.x | `dotnet new blazorserver` template. |
| **JSON Deserialization** | `System.Text.Json` | Built-in (.NET 8) | No Newtonsoft needed. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. |
| **Configuration** | `IConfiguration` + custom JSON | Built-in | Load `data.json` via `builder.Configuration.AddJsonFile("data.json")` or manual `File.ReadAllText`. |
| **Data Models** | C# records | Built-in | Immutable POCOs: `record Milestone(string Name, DateOnly Date, string Type)`, etc. |
| **File Watching** | `FileSystemWatcher` | Built-in | Optional: hot-reload dashboard when `data.json` changes. |

### Project Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Data/
│       │   ├── data.json                    # The config file
│       │   ├── DashboardData.cs             # C# record models
│       │   └── DashboardDataService.cs      # Loads & deserializes JSON
│       ├── Components/
│       │   └── Pages/
│       │       ├── Dashboard.razor           # The single page
│       │       └── Dashboard.razor.css       # Scoped styles from reference
│       └── wwwroot/
│           └── css/
│               └── app.css                   # Minimal global resets
```

### Testing (Minimal, Proportionate to Scope)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | Test JSON deserialization, date calculations, status categorization. |
| **Blazor Component Testing** | bUnit | 1.25+ | Optional. Test that Razor components render correct SVG/HTML for given data. |
| **Assertions** | FluentAssertions | 6.12+ | Readable test assertions. |

### Build & Run

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Build** | `dotnet build` | Standard .NET CLI. |
| **Run** | `dotnet run` | Launches Kestrel on `https://localhost:5001`. |
| **Publish** | `dotnet publish -c Release` | Self-contained optional: `--self-contained -r win-x64`. |

---

## 4. Architecture Recommendations

### Pattern: Minimal Service Architecture

This project does **not** need Clean Architecture, CQRS, MediatR, or repository patterns. It is a single-page read-only dashboard. The appropriate architecture is:

```
data.json → DashboardDataService → Dashboard.razor → HTML/CSS/SVG output
```

Three layers, three files of logic. Anything more is over-engineering.

### Data Flow

1. **Startup:** `DashboardDataService` reads `data.json` from disk and deserializes into `DashboardData` record.
2. **Request:** Blazor Server renders `Dashboard.razor`, injecting `DashboardDataService` via DI.
3. **Render:** Razor component iterates over milestones, status items, and months to produce the grid and SVG timeline.
4. **Refresh:** User refreshes browser (or optionally, `FileSystemWatcher` triggers `StateHasChanged()`).

### Data Model Design (`data.json`)

```json
{
  "title": "Project Phoenix Release Roadmap",
  "organization": "Engineering Platform • Core Services Workstream",
  "reportMonth": "April 2026",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "milestoneStreams": [
    {
      "id": "M1",
      "name": "API Gateway & Auth",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-15", "label": "Kickoff", "type": "checkpoint" },
        { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production" }
      ]
    }
  ],
  "months": ["January", "February", "March", "April"],
  "currentMonth": "April",
  "statusRows": [
    {
      "category": "shipped",
      "label": "Shipped",
      "items": {
        "January": ["OAuth2 flow implemented", "API rate limiting"],
        "February": ["Token refresh logic"],
        "March": ["Load test harness"],
        "April": ["Prod deployment scripts"]
      }
    },
    {
      "category": "in-progress",
      "label": "In Progress",
      "items": { ... }
    },
    {
      "category": "carryover",
      "label": "Carryover",
      "items": { ... }
    },
    {
      "category": "blockers",
      "label": "Blockers",
      "items": { ... }
    }
  ]
}
```

### SVG Timeline Generation Strategy

The reference design maps dates to x-coordinates on a 1560px-wide SVG. The C# logic:

```csharp
double DateToX(DateOnly date, DateOnly start, DateOnly end, double width)
    => (date.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber) * width;
```

Milestone types map to SVG shapes:
- **Checkpoint** → `<circle>` (small, gray fill)
- **PoC Milestone** → `<polygon>` diamond (gold `#F4B400`)
- **Production Release** → `<polygon>` diamond (green `#34A853`)
- **"Now" line** → `<line>` dashed red (`#EA4335`)

### CSS Architecture

Port the reference CSS directly into `Dashboard.razor.css` as scoped styles. Key decisions:

- **Fixed viewport:** Keep `width: 1920px; height: 1080px; overflow: hidden` on the body for screenshot fidelity.
- **Color system:** Define CSS custom properties for the four status categories:
  ```css
  :root {
    --shipped-bg: #F0FBF0; --shipped-accent: #34A853; --shipped-header: #E8F5E9;
    --progress-bg: #EEF4FE; --progress-accent: #0078D4; --progress-header: #E3F2FD;
    --carry-bg: #FFFDE7; --carry-accent: #F4B400; --carry-header: #FFF8E1;
    --block-bg: #FFF5F5; --block-accent: #EA4335; --block-header: #FEF2F2;
  }
  ```
- **Current month highlight:** The `apr` class adds a slightly darker background. Generalize this to `.current-month` applied dynamically based on `data.json`.

### Improvements Over Reference Design

The reference HTML is static. Data-driven Blazor improvements:

1. **Dynamic column count:** Months array in JSON drives grid columns, not hardcoded to 4.
2. **Auto "Now" line positioning:** Calculated from `DateTime.Now` vs. timeline range.
3. **Empty state handling:** If a status category has no items for a month, render a subtle "—" instead of blank.
4. **Overflow truncation:** Long item text gets `text-overflow: ellipsis` to prevent cell blowout.
5. **Print-friendly:** Add `@media print` styles that hide browser chrome for direct-to-PDF screenshot alternative.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope per requirements. The app runs locally on `localhost`. If future access control is ever needed, the simplest addition would be a shared `?token=<guid>` query parameter check in middleware—but do not build this now.

### Data Protection

- `data.json` contains no PII or secrets—it's project status information.
- No encryption needed for data at rest or in transit (localhost only).
- If `data.json` is checked into a repo, ensure no sensitive content leaks (reviewer names, internal codenames that are confidential).

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local Kestrel server via `dotnet run` |
| **Port** | `https://localhost:5001` (default) or configure in `launchSettings.json` |
| **Deployment** | `dotnet publish -c Release -o ./publish` → run `ReportingDashboard.exe` |
| **Self-Contained** | Optional: `dotnet publish -r win-x64 --self-contained` for machines without .NET runtime |
| **Docker** | Not needed. This is a local tool. |
| **CI/CD** | Not needed. Manual build-and-run. |

### Infrastructure Costs

**$0.** This runs entirely on the developer's local machine. No cloud resources, no database server, no hosted services.

---

## 6. Risks & Trade-offs

### Risk: Screenshot Fidelity Across Machines

**Impact:** Medium | **Likelihood:** Medium

The design uses `Segoe UI` font and a fixed 1920×1080 viewport. Screenshots taken on machines with different DPI scaling, browser zoom levels, or missing Segoe UI font will look different.

**Mitigation:** Document the exact screenshot procedure: Chrome, 100% zoom, 1920×1080 window, Windows OS. Alternatively, add a `/screenshot` endpoint using Playwright to programmatically capture pixel-perfect PNGs (but this adds significant complexity—defer unless needed).

### Risk: Blazor Server Overhead for a Static Page

**Impact:** Low | **Likelihood:** Certain

Blazor Server maintains a SignalR WebSocket connection for interactivity. This page has zero interactivity—it's read-only. The SignalR circuit adds ~50KB of JS and a persistent connection.

**Trade-off accepted:** The overhead is negligible for a local single-user tool. The alternative (Blazor Static SSR or raw Razor Pages) would reduce overhead but isn't worth the project restructuring. Blazor Server works fine and the `@rendermode` can be set to static SSR in .NET 8 if desired (`@attribute [StreamRendering]` is not needed here).

**Alternative consideration:** If truly zero JS is desired, use a Razor Pages project (`dotnet new webapp`) instead of Blazor. The Razor syntax is nearly identical. This is a valid simplification but not a stack change—both are .NET 8.

### Risk: JSON Schema Evolution

**Impact:** Low | **Likelihood:** Medium

As the dashboard evolves (new status categories, additional timeline streams), the `data.json` schema will change. Without validation, malformed JSON will produce runtime exceptions.

**Mitigation:** Use `JsonSerializer.Deserialize<DashboardData>()` with strict typing. Add a `try-catch` at load time with a friendly error page showing what's wrong. Consider adding a `"schemaVersion": 1` field to `data.json` for future compatibility.

### Risk: Over-Engineering Temptation

**Impact:** High | **Likelihood:** High

The biggest risk to this project is adding unnecessary complexity: auth, databases, real-time updates, component libraries, multi-page navigation, build pipelines, Docker containers. The user explicitly wants "super simple" and "screenshot for PowerPoint."

**Mitigation:** Enforce a constraint: the entire app should be understandable by reading 3 files (`data.json`, `DashboardDataService.cs`, `Dashboard.razor`). If a feature requires a fourth file of logic, question whether it's needed.

---

## 7. Open Questions

1. **How many milestone streams?** The reference has 3 (M1, M2, M3). Should `data.json` support arbitrary N, or is 3 the fixed maximum? (Recommendation: support N, but test with 3–5.)

2. **How many months displayed?** Reference shows 4 columns (Jan–Apr). Should this always show the last 4 months, or is it configurable? (Recommendation: configurable in `data.json`.)

3. **Who edits `data.json`?** If the user edits it manually, keep the schema flat and simple. If a future tool generates it, the schema can be more structured. (Recommendation: assume manual editing—keep it human-friendly.)

4. **Multiple projects?** Will there ever be multiple dashboards for different projects? If so, support multiple JSON files (`project-phoenix.json`, `project-atlas.json`) selected via URL route. (Recommendation: defer—start with single `data.json`.)

5. **Screenshot automation?** Should the app include a "download as PNG" button? This would require Playwright or a headless browser dependency. (Recommendation: defer—use browser screenshot or Snipping Tool for now.)

---

## 8. Implementation Recommendations

### Phase 1: Core Dashboard (Target: 2–3 hours)

1. `dotnet new blazorserver -n ReportingDashboard -f net8.0`
2. Create `data.json` with fictional project data matching the reference design's structure.
3. Create `DashboardData.cs` with C# record types matching the JSON schema.
4. Create `DashboardDataService.cs` that reads and deserializes the JSON file.
5. Replace the default `Home.razor` with `Dashboard.razor`, porting the reference HTML structure into Razor with `@foreach` loops over the data.
6. Port the reference CSS into `Dashboard.razor.css` as scoped styles.
7. Verify visual parity with the reference design at 1920×1080.

### Phase 2: Data-Driven Improvements (Target: 1–2 hours)

1. Dynamic "Now" line calculated from system date.
2. Dynamic month columns driven by JSON (not hardcoded to 4).
3. Current-month highlight applied automatically.
4. Empty cell graceful handling.
5. Tooltip or subtle metadata (e.g., item count per cell).

### Phase 3: Polish (Target: 1 hour, optional)

1. Print stylesheet for clean PDF export.
2. `FileSystemWatcher` for live reload when `data.json` changes.
3. Error page for malformed JSON with helpful message.
4. README with screenshot procedure documentation.

### Quick Wins

- **Fastest path to value:** Skip Phase 2 and 3 entirely. Phase 1 alone produces a functional, screenshot-ready dashboard. The reference design is already good—data-driving it is the only essential improvement.
- **Copy-paste the CSS:** Don't rewrite the reference design's styles. Copy them verbatim into the scoped CSS file, then parameterize only what needs to be dynamic (month count, current month class).
- **Use records everywhere:** `record Milestone(string Label, string Date, string Type)` is one line of code and gives you immutability, value equality, and `ToString()` for free.

### NuGet Packages Required

**Zero.** The entire project uses only what ships in the .NET 8 SDK:

- `Microsoft.AspNetCore.App` (implicit framework reference)
- `System.Text.Json` (included in framework)

No additional NuGet packages are needed. This is the simplest possible dependency footprint.

### Key Commands

```bash
# Create solution
dotnet new sln -n ReportingDashboard
dotnet new blazorserver -n ReportingDashboard -f net8.0 -o src/ReportingDashboard
dotnet sln add src/ReportingDashboard/ReportingDashboard.csproj

# Build
dotnet build

# Run (launches browser at https://localhost:5001)
dotnet run --project src/ReportingDashboard

# Publish self-contained executable
dotnet publish src/ReportingDashboard -c Release -r win-x64 --self-contained -o ./publish
```

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
