# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 02:15 UTC_

### Summary

This project requires a simple, local-only executive reporting dashboard built on C# .NET 8 with Blazor Server to visualize project milestones and progress across four categorical statuses (Shipped, In Progress, Carryover, Blockers) organized by time periods. The recommended approach is a lightweight Blazor Server application with server-side rendering of a timeline visualization (SVG) and status heatmap (CSS Grid), reading configuration and data from a local JSON file. This stack prioritizes simplicity, screenshot-friendliness, and rapid iteration—no cloud infrastructure, authentication, or complex features required. Estimated implementation time: 1-2 weeks for MVP with example data. --- | Layer | Technology | Version | Key Dependencies | |-------|-----------|---------|------------------| | **UI Framework** | Blazor Server | .NET 8.0 | None | | **Backend Runtime** | .NET 8.0 | 8.0 LTS | System.Text.Json (built-in) | | **Styling** | CSS Grid + Flexbox | Latest | None | | **SVG Rendering** | Blazor `MarkupString` | .NET 8.0 | None | | **Testing** | xUnit + bUnit | Latest | (Optional) | | **Build Tool** | .NET CLI + MSBuild | .NET 8.0 | None | --- This project is an ideal fit for Blazor Server on .NET 8.0. The simple, static-HTML design translates directly to Blazor components with minimal CSS/SVG work. JSON-based configuration keeps deployment friction low. The lack of real-time updates, authentication, and scalability requirements makes this a straightforward monolith with no cloud infrastructure overhead. The team can ship an MVP in 1-2 weeks with zero external dependencies, taking clean screenshots for executive decks immediately. Future enhancements (multi-project, interactivity, audit logging) are deferred post-launch.

### Key Findings

- **Blazor Server is ideal for this use case**: Server-side rendering provides clean, exportable HTML/CSS output suitable for PowerPoint screenshots. No JavaScript framework complexity needed.
- **SVG timeline and CSS Grid heatmap directly port from the HTML design reference**: The OriginalDesignConcept.html uses standard CSS/SVG patterns that Blazor components can easily replicate with dynamic data binding.
- **JSON config file approach keeps deployment minimal**: No database, no migrations, no deployment complexity—store milestones, categories, and time-period data in a simple data.json file read on startup.
- **Chart.js or similar charting libraries not needed**: The design is intentionally simple and uses native SVG and CSS Grid; adding a heavy charting library introduces unnecessary dependencies.
- **Styling with CSS utilities and inline Blazor component styles avoids Build tool overhead**: Component-scoped CSS in .razor files keeps styles maintainable without webpack or PostCSS.
- **No real-time updates or WebSocket complexity**: Since this is a static reporting dashboard updated monthly/quarterly, polling or manual refresh is sufficient.
- **Screenshot optimization**: Blazor's server-side rendering makes it trivial to add a "Print/Export" button that exports clean PNG/SVG directly without browser print dialogs. ---
- Create Blazor Server project template:
   ```bash
   dotnet new blazorserver -n MyProject.Reporting
   ```
- Create `data.json` with example data (copy from design context):
- 4 time periods (Jan, Feb, Mar, Apr)
- 4 status categories (Shipped, InProgress, Carryover, Blockers)
- 3 milestones (M1, M2, M3)
- 4-5 sample items per category
- Implement `ReportDataService`:
- Load JSON from filesystem.
- Deserialize into `ProjectReport` DTO.
- Handle errors gracefully (invalid JSON → error message).
- Create data models:
   ```csharp
   public class ProjectReport { string ProjectName, string Context, List<Milestone>, Dictionary<string, Category> }
   public class Milestone { string Name, string Color, int StartMonth, int EndMonth }
   public class Category { string Color, List<StatusItem> Items }
   public class StatusItem { string Name, List<bool> Periods }
   ```
- Create `TimelineComponent.razor`:
- Input: Milestones, time periods.
- Generate SVG inline: month dividers, milestone markers, checkpoints.
- Use `@((MarkupString)svgContent)` to render.
- Reference CSS from design (colors, fonts, spacing).
- Create `HeatmapComponent.razor`:
- Input: Status categories, items, time periods.
- CSS Grid: 160px left column + `repeat(N, 1fr)` for months.
- Row headers: category names with background colors.
- Cells: colored backgrounds + item names with dots.
- Scoped CSS for styling.
- Create `DashboardPage.razor`:
- Inject `ReportDataService`.
- Fetch data on `OnInitializedAsync()`.
- Render header, timeline, heatmap, legend.
- Add "Print" button (browser print).
- Style header & legend:
- Title, subtitle, legend box (copied from design).
- Responsive flex layout.
- Verify visual match with `OriginalDesignConcept.html`:
- Colors, fonts, spacing.
- Take screenshot; compare with design.
- Iterate CSS until pixel-perfect (or close).
- Unit tests for `ReportDataService`:
- Valid JSON → correct deserialization.
- Invalid JSON → graceful error.
- Missing fields → defaults or error.
- Component tests (bUnit):
- TimelineComponent: milestone count, correct positioning.
- HeatmapComponent: grid dimensions, cell values.
- Browser testing:
- Chrome, Edge, Firefox, Safari (if on Mac).
- Print to PDF → verify layout.
- Documentation:
- README: How to update data.json.
- data.json schema comments.
- Deployment guide.
- **Day 1-2**: Functional app with hardcoded example data (no JSON parsing) running locally.
- **Day 3**: Live JSON loading; users can edit data.json and refresh.
- **Day 4**: First screenshot-ready MVP with visual polish.
- **SVG Timeline complexity**: If timeline has 100+ milestones or custom shapes, prototype in standalone SVG before integrating into Blazor.
- **Heatmap cell interactions**: If executives want hover tooltips, drill-down, or drill, prototype Blazor component event handling in isolation.
- **Export to PNG**: If programmatic screenshot needed, prototype Playwright or headless Chrome integration separately before adding to core app.
- **Multi-project support**: Data structure → list of reports; dropdown selector.
- **Real-time data sync**: File watcher on data.json; auto-reload (requires refactoring service).
- **Historical tracking**: Archive data.json snapshots; comparison view.
- **Interactive drill-down**: Click category → detail view of items.
- **Theme switcher**: Dark mode, high-contrast mode.
- **Audit log**: Who changed data.json and when (Git history integration). ---

### Recommended Tools & Technologies

- | Component | Technology | Version | Rationale | |-----------|-----------|---------|-----------| | UI Framework | Blazor Server | .NET 8.0 | Built-in, server-side rendering, no JavaScript required | | Component Library | None (custom) | N/A | Design is simple enough that custom components suffice; avoid unnecessary dependencies | | CSS Layout | Native CSS Grid + Flexbox | Latest | Timeline and heatmap grids use standard CSS; no Tailwind or Bootstrap needed for simplicity | | SVG Rendering | Razor + `MarkupString` | N/A | Inline SVG generation in Blazor components for timeline visualization | | Icons/Assets | SVG inline | N/A | Simple colored squares and lines for legend; inline minimizes HTTP requests | | Component | Technology | Version | Rationale | |-----------|-----------|---------|-----------| | Runtime | .NET 8.0 | 8.0 LTS | Latest stable, long-term support, best performance | | Configuration | System.Text.Json | .NET 8.0 built-in | Native JSON serialization; no Newtonsoft.Json dependency for simplicity | | Data Source | Local JSON file | N/A | Single `data.json` file in app root; no database needed | | Services | C# classes + dependency injection | .NET 8.0 built-in | Simple service pattern for parsing and aggregating data | | Layer | Format | Location | Notes | |-------|--------|----------|-------| | **Milestones** | JSON object array | `data.json` | `{ name, color, dateStart, dateEnd }` | | **Status Categories** | JSON object map | `data.json` | `{ Shipped, InProgress, Carryover, Blockers }` with items and colors | | **Time Periods** | JSON string array | `data.json` | Months or quarters; matches heatmap column headers | | Tool | Version | Purpose | |------|---------|---------| | Visual Studio 2022+ or VS Code + C# extension | Latest | IDE for development | | .NET CLI | 8.0 | Build, run, publish | | dotnet new blazorserver | N/A | Project template | | No additional build tools needed | N/A | Blazor Server projects compile with standard MSBuild | | Framework | Version | Scope | |-----------|---------|-------| | xUnit or NUnit | Latest | Unit tests for data parsing logic (minimal) | | Blazor testing library (bUnit) | Latest | Component integration tests (if needed for future enhancements) | ---
```
┌─────────────────────────────────┐
│  Blazor Server Pages & Components │  (UI Layer)
│  - DashboardPage.razor           │
│  - TimelineComponent.razor       │
│  - HeatmapComponent.razor        │
├─────────────────────────────────┤
│  Services (Business Logic)        │  (Application Layer)
│  - ReportDataService             │
│  - DataParser                    │
├─────────────────────────────────┤
│  Data Models (DTOs)              │  (Domain Layer)
│  - ProjectReport                 │
│  - Milestone, StatusItem         │
├─────────────────────────────────┤
│  Data Source                     │  (Persistence Layer)
│  - data.json (Local File)        │
└─────────────────────────────────┘
```
- **Application Startup**:
- `Program.cs` registers `ReportDataService` in DI container.
- Service loads `data.json` from filesystem on first request.
- JSON deserialized into strongly-typed C# objects.
- **Page Load** (`DashboardPage.razor`):
- Inject `ReportDataService`.
- Call `GetReport()` synchronously (data is small, no async needed).
- Pass data to child components.
- **Rendering**:
- **Timeline**: Generate SVG markup dynamically based on milestone objects.
- **Heatmap**: Generate CSS Grid with data-bound cell values from status arrays.
- **User Interactions** (minimal):
- Export/Print button triggers browser print or JavaScript to render PNG.
```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "projectContext": "Trusted Platform – Privacy Automation Workstream – April 2026",
  "timePeriods": ["January", "February", "March", "April"],
  "milestones": [
    {
      "id": "m1",
      "name": "Chatbot & MS Role",
      "color": "#0078D4",
      "startMonth": 0,
      "endMonth": 1,
      "type": "checkpoint"
    }
  ],
  "statusCategories": {
    "Shipped": {
      "color": "#1B7A28",
      "bgColor": "#E8F5E9",
      "items": [
        { "name": "Feature A", "periods": ["x", "x", "", "x"] }
      ]
    },
    "InProgress": { ... },
    "Carryover": { ... },
    "Blockers": { ... }
  }
}
``` **DashboardPage.razor** (Page Container)
- Fetches data
- Renders header, timeline, heatmap
- Exports button handler **TimelineComponent.razor** (SVG Timeline)
- Input: List of milestones, time periods
- Output: Rendered SVG with month divisions, milestone markers, legend
- No external SVG library; inline generation with `@((MarkupString)svgContent)` **HeatmapComponent.razor** (Status Grid)
- Input: Status categories, items, time periods
- Output: CSS Grid with colored cells
- Scoped CSS for styling **StatusLegend.razor** (Legend Box)
- Input: Milestone types, colors
- Output: Colored squares + labels | Decision | Reasoning | |----------|-----------| | **Blazor Server, not Blazor WASM** | Server-side rendering produces clean, exportable HTML; no JavaScript runtime needed | | **No real-time updates** | Dashboard is read-only, updated monthly/quarterly manually | | **JSON file, not database** | Simplicity; avoids SQL Server or SQLite setup; data.json is version-controllable | | **Component-scoped CSS** | Keeps styles local to components; avoids global CSS conflicts | | **No charting libraries** | Design uses simple SVG and CSS Grid; libraries like Chart.js add bloat | | **Manual data refresh** | On deployment, data.json is updated; no hot reload needed | ---

### Considerations & Risks

- **Not required for MVP**. This is an internal reporting dashboard with no multi-user access control or sensitive data exposure. **If needed in future**:
- Use `[Authorize]` attribute on Blazor components with ASP.NET Core Identity (local user DB).
- Avoid external OAuth providers (cloud-only; violates local-only constraint).
- **JSON data**: No encryption needed (example/test data only); if sensitive, use file permissions (Windows ACLs) to restrict access.
- **HTTPS**: Not required locally; HTTP on `localhost:5000` is sufficient.
- **GDPR/Compliance**: Dashboard contains only fictional milestone data; no PII or regulated data. | Aspect | Approach | Notes | |--------|----------|-------| | **Hosting** | Local machine or internal network only | No cloud provider (AWS, Azure cloud); standard ASP.NET Core hosting on Windows/Linux | | **Containerization** | Optional Docker for portability | `Dockerfile` can package app for consistent deployment across machines | | **Database** | None | File-based JSON suffices | | **Caching** | In-memory (ReportDataService singleton) | Load data once on startup; minimal memory footprint | | **Logging** | Console/text file via Serilog (optional) | Minimal observability needed; basic console output sufficient for debugging |
- **Development**:
   ```bash
   dotnet watch run
   ``` App starts on `https://localhost:7123` (default Blazor Server port).
- **Production Packaging**:
   ```bash
   dotnet publish -c Release -o ./publish
   ``` Deploy `publish/` folder to target machine.
- **Run on Target Machine**:
   ```bash
   dotnet MyProject.dll
   ``` App listens on configured port (default `5000`/`5001`).
- **Update Data**:
- Replace `data.json` in app root.
- Restart application (or implement file watcher for auto-reload, but not required for MVP).
- **Data backups**: Keep version-controlled copy of data.json in Git for audit trail.
- **Accessibility**: Ensure text contrast and ARIA labels meet WCAG 2.1 AA for executive dashboard visibility.
- **Browser support**: Blazor Server requires modern browser (Chrome, Edge, Safari, Firefox); IE11 not supported (acceptable for modern org).
- **Port conflicts**: Default ports may already be in use; configuration in `appsettings.json` allows customization. --- | Risk | Severity | Mitigation | |------|----------|-----------| | **JSON file parsing errors** | Medium | Implement validation in `ReportDataService`; unit tests for malformed JSON; schema comments in data.json template | | **Large dataset performance** | Low | Unlikely; typical executive dashboards have 10-20 items. If data grows, consider caching or pagination. | | **Blazor Server websocket connection loss** | Low | Rare in local/internal network. Blazor auto-reconnects; user sees "reconnecting" message briefly. Acceptable for internal tool. | | **SVG rendering complexity** | Low | Keep timeline simple (50-60 month range max). If more complex, consider D3.js or Plotly, but adds dependency overhead. |
- **Single data.json file**: If updating from multiple sources, implement file locking or use a database (violates "local-only" constraint; deferred).
- **In-memory caching**: Data loaded once per app restart; for frequently updated dashboards, implement file watcher + reload logic.
- **Blazor Server session handling**: Each connected user requires a persistent server connection; if >100 concurrent users, consider Blazor WASM or API + SPA (out of scope). | Trade-off | Choice | Rationale | |-----------|--------|-----------| | **Simplicity vs. Flexibility** | Simplicity | No extensible plugin system, no multi-datasource abstraction; hardcoded to JSON structure. Future changes require code updates. | | **Performance vs. Development Speed** | Development Speed | No caching layer, no CDN, no async/await optimization. Good enough for <100 concurrent dashboard users. | | **Visual Fidelity vs. Shipping Time** | Shipping Time | CSS Grid + SVG replication of design is straightforward; skip complex animation or interactive brushing. | | **Local-only vs. Scalability** | Local-only | Deployment to cloud (Azure, AWS) is precluded; team must manage hosting infrastructure. |
- **Blazor + C#**: Team must know C# and ASP.NET Core basics; Blazor learning curve is 2-3 days for experienced .NET developers.
- **SVG generation**: Custom SVG markup in C# strings is unfamiliar to some; recommend pairing with SVG library reference or D3 examples for learning.
- **CSS Grid + Flexbox**: Assumed knowledge; brief training if team is CSS-unfamiliar. ---
- **How frequently is the dashboard data updated?**
- Monthly? Quarterly? Weekly?
- Drives decision on file watcher vs. manual restart.
- **Who updates data.json?**
- Product Manager directly edits JSON?
- Excel export → JSON conversion script?
- Impacts training and error handling.
- **How many projects should the dashboard support?**
- Single hardcoded project (current design)?
- Multi-project dropdown selector?
- Drives data structure and UI complexity.
- **Export/print requirements?**
- Browser print to PDF (simple)?
- Programmatic PNG screenshot (requires Playwright or Selenium)?
- Affects MVP scope.
- **Accessibility requirements?**
- Internal tool only (relax WCAG)?
- Executive audience with visual impairments (strict WCAG AA)?
- Impacts component design and testing.
- **Deployment environment?**
- Windows Server / on-prem?
- Docker container in Kubernetes?
- Development machine only?
- Drives Docker/k8s investment decision. ---

### Detailed Analysis

# Research: Executive Reporting Dashboard Technology Stack

## Executive Summary

This project requires a simple, local-only executive reporting dashboard built on C# .NET 8 with Blazor Server to visualize project milestones and progress across four categorical statuses (Shipped, In Progress, Carryover, Blockers) organized by time periods. The recommended approach is a lightweight Blazor Server application with server-side rendering of a timeline visualization (SVG) and status heatmap (CSS Grid), reading configuration and data from a local JSON file. This stack prioritizes simplicity, screenshot-friendliness, and rapid iteration—no cloud infrastructure, authentication, or complex features required. Estimated implementation time: 1-2 weeks for MVP with example data.

---

## Key Findings

- **Blazor Server is ideal for this use case**: Server-side rendering provides clean, exportable HTML/CSS output suitable for PowerPoint screenshots. No JavaScript framework complexity needed.
- **SVG timeline and CSS Grid heatmap directly port from the HTML design reference**: The OriginalDesignConcept.html uses standard CSS/SVG patterns that Blazor components can easily replicate with dynamic data binding.
- **JSON config file approach keeps deployment minimal**: No database, no migrations, no deployment complexity—store milestones, categories, and time-period data in a simple data.json file read on startup.
- **Chart.js or similar charting libraries not needed**: The design is intentionally simple and uses native SVG and CSS Grid; adding a heavy charting library introduces unnecessary dependencies.
- **Styling with CSS utilities and inline Blazor component styles avoids Build tool overhead**: Component-scoped CSS in .razor files keeps styles maintainable without webpack or PostCSS.
- **No real-time updates or WebSocket complexity**: Since this is a static reporting dashboard updated monthly/quarterly, polling or manual refresh is sufficient.
- **Screenshot optimization**: Blazor's server-side rendering makes it trivial to add a "Print/Export" button that exports clean PNG/SVG directly without browser print dialogs.

---

## Recommended Technology Stack

### Frontend Layer

| Component | Technology | Version | Rationale |
|-----------|-----------|---------|-----------|
| UI Framework | Blazor Server | .NET 8.0 | Built-in, server-side rendering, no JavaScript required |
| Component Library | None (custom) | N/A | Design is simple enough that custom components suffice; avoid unnecessary dependencies |
| CSS Layout | Native CSS Grid + Flexbox | Latest | Timeline and heatmap grids use standard CSS; no Tailwind or Bootstrap needed for simplicity |
| SVG Rendering | Razor + `MarkupString` | N/A | Inline SVG generation in Blazor components for timeline visualization |
| Icons/Assets | SVG inline | N/A | Simple colored squares and lines for legend; inline minimizes HTTP requests |

### Backend Layer

| Component | Technology | Version | Rationale |
|-----------|-----------|---------|-----------|
| Runtime | .NET 8.0 | 8.0 LTS | Latest stable, long-term support, best performance |
| Configuration | System.Text.Json | .NET 8.0 built-in | Native JSON serialization; no Newtonsoft.Json dependency for simplicity |
| Data Source | Local JSON file | N/A | Single `data.json` file in app root; no database needed |
| Services | C# classes + dependency injection | .NET 8.0 built-in | Simple service pattern for parsing and aggregating data |

### Data & Configuration

| Layer | Format | Location | Notes |
|-------|--------|----------|-------|
| **Milestones** | JSON object array | `data.json` | `{ name, color, dateStart, dateEnd }` |
| **Status Categories** | JSON object map | `data.json` | `{ Shipped, InProgress, Carryover, Blockers }` with items and colors |
| **Time Periods** | JSON string array | `data.json` | Months or quarters; matches heatmap column headers |

### Development & Build Tools

| Tool | Version | Purpose |
|------|---------|---------|
| Visual Studio 2022+ or VS Code + C# extension | Latest | IDE for development |
| .NET CLI | 8.0 | Build, run, publish |
| dotnet new blazorserver | N/A | Project template |
| No additional build tools needed | N/A | Blazor Server projects compile with standard MSBuild |

### Testing & Quality (Optional but Recommended)

| Framework | Version | Scope |
|-----------|---------|-------|
| xUnit or NUnit | Latest | Unit tests for data parsing logic (minimal) |
| Blazor testing library (bUnit) | Latest | Component integration tests (if needed for future enhancements) |

---

## Architecture Recommendations

### Overall Architecture: Layered Monolith

```
┌─────────────────────────────────┐
│  Blazor Server Pages & Components │  (UI Layer)
│  - DashboardPage.razor           │
│  - TimelineComponent.razor       │
│  - HeatmapComponent.razor        │
├─────────────────────────────────┤
│  Services (Business Logic)        │  (Application Layer)
│  - ReportDataService             │
│  - DataParser                    │
├─────────────────────────────────┤
│  Data Models (DTOs)              │  (Domain Layer)
│  - ProjectReport                 │
│  - Milestone, StatusItem         │
├─────────────────────────────────┤
│  Data Source                     │  (Persistence Layer)
│  - data.json (Local File)        │
└─────────────────────────────────┘
```

### Data Flow

1. **Application Startup**: 
   - `Program.cs` registers `ReportDataService` in DI container.
   - Service loads `data.json` from filesystem on first request.
   - JSON deserialized into strongly-typed C# objects.

2. **Page Load** (`DashboardPage.razor`):
   - Inject `ReportDataService`.
   - Call `GetReport()` synchronously (data is small, no async needed).
   - Pass data to child components.

3. **Rendering**:
   - **Timeline**: Generate SVG markup dynamically based on milestone objects.
   - **Heatmap**: Generate CSS Grid with data-bound cell values from status arrays.

4. **User Interactions** (minimal):
   - Export/Print button triggers browser print or JavaScript to render PNG.

### Data Structure Example

```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "projectContext": "Trusted Platform – Privacy Automation Workstream – April 2026",
  "timePeriods": ["January", "February", "March", "April"],
  "milestones": [
    {
      "id": "m1",
      "name": "Chatbot & MS Role",
      "color": "#0078D4",
      "startMonth": 0,
      "endMonth": 1,
      "type": "checkpoint"
    }
  ],
  "statusCategories": {
    "Shipped": {
      "color": "#1B7A28",
      "bgColor": "#E8F5E9",
      "items": [
        { "name": "Feature A", "periods": ["x", "x", "", "x"] }
      ]
    },
    "InProgress": { ... },
    "Carryover": { ... },
    "Blockers": { ... }
  }
}
```

### Component Structure

**DashboardPage.razor** (Page Container)
- Fetches data
- Renders header, timeline, heatmap
- Exports button handler

**TimelineComponent.razor** (SVG Timeline)
- Input: List of milestones, time periods
- Output: Rendered SVG with month divisions, milestone markers, legend
- No external SVG library; inline generation with `@((MarkupString)svgContent)`

**HeatmapComponent.razor** (Status Grid)
- Input: Status categories, items, time periods
- Output: CSS Grid with colored cells
- Scoped CSS for styling

**StatusLegend.razor** (Legend Box)
- Input: Milestone types, colors
- Output: Colored squares + labels

### Key Decisions

| Decision | Reasoning |
|----------|-----------|
| **Blazor Server, not Blazor WASM** | Server-side rendering produces clean, exportable HTML; no JavaScript runtime needed |
| **No real-time updates** | Dashboard is read-only, updated monthly/quarterly manually |
| **JSON file, not database** | Simplicity; avoids SQL Server or SQLite setup; data.json is version-controllable |
| **Component-scoped CSS** | Keeps styles local to components; avoids global CSS conflicts |
| **No charting libraries** | Design uses simple SVG and CSS Grid; libraries like Chart.js add bloat |
| **Manual data refresh** | On deployment, data.json is updated; no hot reload needed |

---

## Security & Infrastructure

### Authentication & Authorization

**Not required for MVP**. This is an internal reporting dashboard with no multi-user access control or sensitive data exposure.

**If needed in future**:
- Use `[Authorize]` attribute on Blazor components with ASP.NET Core Identity (local user DB).
- Avoid external OAuth providers (cloud-only; violates local-only constraint).

### Data Protection

- **JSON data**: No encryption needed (example/test data only); if sensitive, use file permissions (Windows ACLs) to restrict access.
- **HTTPS**: Not required locally; HTTP on `localhost:5000` is sufficient.
- **GDPR/Compliance**: Dashboard contains only fictional milestone data; no PII or regulated data.

### Infrastructure & Deployment

| Aspect | Approach | Notes |
|--------|----------|-------|
| **Hosting** | Local machine or internal network only | No cloud provider (AWS, Azure cloud); standard ASP.NET Core hosting on Windows/Linux |
| **Containerization** | Optional Docker for portability | `Dockerfile` can package app for consistent deployment across machines |
| **Database** | None | File-based JSON suffices |
| **Caching** | In-memory (ReportDataService singleton) | Load data once on startup; minimal memory footprint |
| **Logging** | Console/text file via Serilog (optional) | Minimal observability needed; basic console output sufficient for debugging |

### Deployment Steps (Development to Production)

1. **Development**: 
   ```bash
   dotnet watch run
   ```
   App starts on `https://localhost:7123` (default Blazor Server port).

2. **Production Packaging**:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
   Deploy `publish/` folder to target machine.

3. **Run on Target Machine**:
   ```bash
   dotnet MyProject.dll
   ```
   App listens on configured port (default `5000`/`5001`).

4. **Update Data**:
   - Replace `data.json` in app root.
   - Restart application (or implement file watcher for auto-reload, but not required for MVP).

### Operational Concerns

- **Data backups**: Keep version-controlled copy of data.json in Git for audit trail.
- **Accessibility**: Ensure text contrast and ARIA labels meet WCAG 2.1 AA for executive dashboard visibility.
- **Browser support**: Blazor Server requires modern browser (Chrome, Edge, Safari, Firefox); IE11 not supported (acceptable for modern org).
- **Port conflicts**: Default ports may already be in use; configuration in `appsettings.json` allows customization.

---

## Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| **JSON file parsing errors** | Medium | Implement validation in `ReportDataService`; unit tests for malformed JSON; schema comments in data.json template |
| **Large dataset performance** | Low | Unlikely; typical executive dashboards have 10-20 items. If data grows, consider caching or pagination. |
| **Blazor Server websocket connection loss** | Low | Rare in local/internal network. Blazor auto-reconnects; user sees "reconnecting" message briefly. Acceptable for internal tool. |
| **SVG rendering complexity** | Low | Keep timeline simple (50-60 month range max). If more complex, consider D3.js or Plotly, but adds dependency overhead. |

### Scalability Bottlenecks

- **Single data.json file**: If updating from multiple sources, implement file locking or use a database (violates "local-only" constraint; deferred).
- **In-memory caching**: Data loaded once per app restart; for frequently updated dashboards, implement file watcher + reload logic.
- **Blazor Server session handling**: Each connected user requires a persistent server connection; if >100 concurrent users, consider Blazor WASM or API + SPA (out of scope).

### Trade-offs

| Trade-off | Choice | Rationale |
|-----------|--------|-----------|
| **Simplicity vs. Flexibility** | Simplicity | No extensible plugin system, no multi-datasource abstraction; hardcoded to JSON structure. Future changes require code updates. |
| **Performance vs. Development Speed** | Development Speed | No caching layer, no CDN, no async/await optimization. Good enough for <100 concurrent dashboard users. |
| **Visual Fidelity vs. Shipping Time** | Shipping Time | CSS Grid + SVG replication of design is straightforward; skip complex animation or interactive brushing. |
| **Local-only vs. Scalability** | Local-only | Deployment to cloud (Azure, AWS) is precluded; team must manage hosting infrastructure. |

### Skills Gaps

- **Blazor + C#**: Team must know C# and ASP.NET Core basics; Blazor learning curve is 2-3 days for experienced .NET developers.
- **SVG generation**: Custom SVG markup in C# strings is unfamiliar to some; recommend pairing with SVG library reference or D3 examples for learning.
- **CSS Grid + Flexbox**: Assumed knowledge; brief training if team is CSS-unfamiliar.

---

## Open Questions

1. **How frequently is the dashboard data updated?**
   - Monthly? Quarterly? Weekly?
   - Drives decision on file watcher vs. manual restart.

2. **Who updates data.json?**
   - Product Manager directly edits JSON?
   - Excel export → JSON conversion script?
   - Impacts training and error handling.

3. **How many projects should the dashboard support?**
   - Single hardcoded project (current design)?
   - Multi-project dropdown selector?
   - Drives data structure and UI complexity.

4. **Export/print requirements?**
   - Browser print to PDF (simple)?
   - Programmatic PNG screenshot (requires Playwright or Selenium)?
   - Affects MVP scope.

5. **Accessibility requirements?**
   - Internal tool only (relax WCAG)?
   - Executive audience with visual impairments (strict WCAG AA)?
   - Impacts component design and testing.

6. **Deployment environment?**
   - Windows Server / on-prem?
   - Docker container in Kubernetes?
   - Development machine only?
   - Drives Docker/k8s investment decision.

---

## Implementation Recommendations

### MVP Scope (1-2 weeks)

**Phase 1: Foundation (Days 1-3)**

1. Create Blazor Server project template:
   ```bash
   dotnet new blazorserver -n MyProject.Reporting
   ```

2. Create `data.json` with example data (copy from design context):
   - 4 time periods (Jan, Feb, Mar, Apr)
   - 4 status categories (Shipped, InProgress, Carryover, Blockers)
   - 3 milestones (M1, M2, M3)
   - 4-5 sample items per category

3. Implement `ReportDataService`:
   - Load JSON from filesystem.
   - Deserialize into `ProjectReport` DTO.
   - Handle errors gracefully (invalid JSON → error message).

4. Create data models:
   ```csharp
   public class ProjectReport { string ProjectName, string Context, List<Milestone>, Dictionary<string, Category> }
   public class Milestone { string Name, string Color, int StartMonth, int EndMonth }
   public class Category { string Color, List<StatusItem> Items }
   public class StatusItem { string Name, List<bool> Periods }
   ```

**Phase 2: UI Components (Days 4-7)**

1. Create `TimelineComponent.razor`:
   - Input: Milestones, time periods.
   - Generate SVG inline: month dividers, milestone markers, checkpoints.
   - Use `@((MarkupString)svgContent)` to render.
   - Reference CSS from design (colors, fonts, spacing).

2. Create `HeatmapComponent.razor`:
   - Input: Status categories, items, time periods.
   - CSS Grid: 160px left column + `repeat(N, 1fr)` for months.
   - Row headers: category names with background colors.
   - Cells: colored backgrounds + item names with dots.
   - Scoped CSS for styling.

3. Create `DashboardPage.razor`:
   - Inject `ReportDataService`.
   - Fetch data on `OnInitializedAsync()`.
   - Render header, timeline, heatmap, legend.
   - Add "Print" button (browser print).

4. Style header & legend:
   - Title, subtitle, legend box (copied from design).
   - Responsive flex layout.

**Phase 3: Polish & Testing (Days 8-10)**

1. Verify visual match with `OriginalDesignConcept.html`:
   - Colors, fonts, spacing.
   - Take screenshot; compare with design.
   - Iterate CSS until pixel-perfect (or close).

2. Unit tests for `ReportDataService`:
   - Valid JSON → correct deserialization.
   - Invalid JSON → graceful error.
   - Missing fields → defaults or error.

3. Component tests (bUnit):
   - TimelineComponent: milestone count, correct positioning.
   - HeatmapComponent: grid dimensions, cell values.

4. Browser testing:
   - Chrome, Edge, Firefox, Safari (if on Mac).
   - Print to PDF → verify layout.

5. Documentation:
   - README: How to update data.json.
   - data.json schema comments.
   - Deployment guide.

### Quick Wins (Demonstrate Value Early)

- **Day 1-2**: Functional app with hardcoded example data (no JSON parsing) running locally.
- **Day 3**: Live JSON loading; users can edit data.json and refresh.
- **Day 4**: First screenshot-ready MVP with visual polish.

### Areas for Prototyping Before Committing

- **SVG Timeline complexity**: If timeline has 100+ milestones or custom shapes, prototype in standalone SVG before integrating into Blazor.
- **Heatmap cell interactions**: If executives want hover tooltips, drill-down, or drill, prototype Blazor component event handling in isolation.
- **Export to PNG**: If programmatic screenshot needed, prototype Playwright or headless Chrome integration separately before adding to core app.

### Future Enhancements (Out of MVP Scope)

1. **Multi-project support**: Data structure → list of reports; dropdown selector.
2. **Real-time data sync**: File watcher on data.json; auto-reload (requires refactoring service).
3. **Historical tracking**: Archive data.json snapshots; comparison view.
4. **Interactive drill-down**: Click category → detail view of items.
5. **Theme switcher**: Dark mode, high-contrast mode.
6. **Audit log**: Who changed data.json and when (Git history integration).

---

## Technology Stack Summary

| Layer | Technology | Version | Key Dependencies |
|-------|-----------|---------|------------------|
| **UI Framework** | Blazor Server | .NET 8.0 | None |
| **Backend Runtime** | .NET 8.0 | 8.0 LTS | System.Text.Json (built-in) |
| **Styling** | CSS Grid + Flexbox | Latest | None |
| **SVG Rendering** | Blazor `MarkupString` | .NET 8.0 | None |
| **Testing** | xUnit + bUnit | Latest | (Optional) |
| **Build Tool** | .NET CLI + MSBuild | .NET 8.0 | None |

**Total external NuGet dependencies: 0 (everything built-in)**

---

## Conclusion

This project is an ideal fit for Blazor Server on .NET 8.0. The simple, static-HTML design translates directly to Blazor components with minimal CSS/SVG work. JSON-based configuration keeps deployment friction low. The lack of real-time updates, authentication, and scalability requirements makes this a straightforward monolith with no cloud infrastructure overhead. The team can ship an MVP in 1-2 weeks with zero external dependencies, taking clean screenshots for executive decks immediately. Future enhancements (multi-project, interactivity, audit logging) are deferred post-launch.

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
