# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 07:45 UTC_

### Summary

This project requires a lightweight, screenshot-friendly executive dashboard built with C# .NET 8 and Blazor Server. The solution reads project metadata and status from a JSON configuration file and renders a timeline-based reporting view with status heatmap visualization. Given the simplicity requirements (no authentication, local-only deployment, static data binding), Blazor Server with built-in templating and charting libraries is ideally suited. The minimal infrastructure footprint and rapid development velocity make this stack optimal for iterative refinement toward PowerPoint-ready output. Primary risks are Blazor component lifecycle complexity and ensuring visual fidelity to the design mock.

### Key Findings

- Blazor Server eliminates the need for separate frontend frameworks (React, Vue) while maintaining full C# type safety and code reuse; ideal for dashboard work with no authentication overhead.
- SVG-based charting (using a lightweight library like OxyPlot or Chart.js bridge) is preferred over heavy canvas libraries because the design references SVG for timeline visualization.
- CSS Grid and Flexbox are native browser technologies and require no additional libraries; the OriginalDesignConcept.html already demonstrates the exact layout approach needed.
- JSON deserialization in .NET 8 is built-in via `System.Text.Json`; no additional serialization libraries required.
- Blazor's component model maps naturally to the dashboard's visual sections (Header, Timeline, Heatmap), enabling clean separation of concerns.
- Local file I/O via `File.ReadAllTextAsync()` is sufficient for reading data.json; no database infrastructure needed.
- Static site generation is not required; Blazor Server handles dynamic rendering server-side with minimal client-side JavaScript.
- Set up Blazor Server project in Visual Studio.
- Create POCO models matching `data.json` structure.
- Implement `DashboardDataService` with `GetDashboardDataAsync()` (hardcoded `data.json` path).
- Build `DashboardHeader.razor` component (title, subtitle, legend).
- Build `HeatmapGrid.razor` component with CSS Grid layout, static sample data.
- Verify visual layout matches design mock (1920x1080 viewport).
- **Deliverable**: Blazor app running on `localhost:5001`, rendering header + heatmap with dummy data.
- Modify `data.json` to include sample project data (4 status rows, 12 months, 3 milestones).
- Wire `DashboardDataService` to `DashboardDataService` and render real data in `HeatmapGrid.razor`.
- Build `TimelineChart.razor` with OxyPlot or SVG rendering of timeline + milestones.
- Implement color mapping (status category → CSS class).
- **Deliverable**: Dashboard reads and renders full `data.json`, timeline visual matches design.
- Add CSS refinements (fonts, spacing, colors) to match design pixel-perfectly.
- Implement refresh button + error handling for malformed `data.json`.
- Add optional features (filter by category, legend click-to-highlight).
- Deploy as Windows Service or Docker container.
- **Deliverable**: Production-ready dashboard, ready for executive PowerPoint screenshots.
- **Copy design CSS directly**: Extract styles from `OriginalDesignConcept.html` into `app.css`. Saves design iteration time.
- **Hardcode sample data**: Populate `data.json` with 2-3 sample projects. Developers and executives can verify layout without ongoing data integration.
- **Responsive print styles**: Add `@media print` CSS to hide UI chrome (buttons, legends) for clean PowerPoint screenshots.
- **Dark mode toggle** (optional): Add CSS variables for light/dark theme. Easy UI win for executives.
- **Priority 1**: Build static HTML mock in Phase 1 (DashboardHeader + HeatmapGrid) to validate visual fidelity before committing to Blazor. Use `OriginalDesignConcept.html` as template.
- **Priority 2**: Prototype OxyPlot timeline rendering (Phase 2) to ensure SVG output matches design. If OxyPlot output is unsatisfactory, fall back to custom SVG generation or Chart.js.
- **Priority 3**: Test Blazor component refresh performance with large heatmaps (100+ cells) to identify rendering bottlenecks early.

### Recommended Tools & Technologies

- **Blazor Server** (included in .NET 8 SDK, v8.0.0+): Server-side C# rendering with SignalR WebSocket communication. No SPA framework needed. Use default `.razor` component format.
- **Tailwind CSS** (v3.4.0+) or **native CSS**: The design mock uses simple CSS Grid/Flexbox. Tailwind is optional but recommended for utility-first styling consistency. Alternatively, embed CSS directly in Blazor component `.razor` files or a single `app.css`.
- **Chart.js** (v4.4.0+) via Interop or **OxyPlot** (v2.1.2+, NuGet): For timeline and heatmap visualization. OxyPlot is C#-native and Blazor-friendly. Chart.js requires JavaScript interop.
- **Recommendation**: Use OxyPlot for minimal JS dependency overhead; it generates SVG output matching the design aesthetic.
- **FontAwesome** (v6.5.0+, free CDN or NuGet): Optional icon library for legend markers and status indicators. The design uses diamond/circle/line shapes that can be replicated with pure CSS or inline SVG.
- **.NET 8 ASP.NET Core** (v8.0.0+): Built into Blazor Server. No additional web framework needed.
- **System.Text.Json** (included in .NET 8): Deserialize `data.json` into POCO models. No external JSON library required.
- **ILogger** (built-in, included in .NET Generic Host): Minimal logging for debugging file I/O and data parsing.
- **Local JSON file** (`data.json`): Single source of truth. No database. Use `File.ReadAllTextAsync()` from `System.IO` (built-in).
- **POCO Models** (C# classes): Strongly-typed representation of JSON structure. Auto-generated via Visual Studio "Paste JSON as Classes" or manual POCO definition.
- **Local HTTP Server**: Blazor Server runs on `https://localhost:5001` by default (or HTTP on `localhost:5000`). No cloud infrastructure required.
- **Kestrel** (built-in ASP.NET Core web server): Default HTTP server. No IIS or external proxy required.
- **Windows Service or Console App**: For long-term local deployment, wrap the Blazor app as a Windows Service using `TopShelf` (v4.4.1+, NuGet) or run as a background console app.
- **Screenshot Automation**: Use Puppeteer Sharp (v10.0.0+) or Selenium (v4.15.0+) for headless browser screenshots if PowerPoint deck generation is automated. For manual screenshots, Chrome DevTools or `Win+Shift+S` suffice.
- **xUnit** (v2.7.0+) or **NUnit** (v4.0.0+): Unit testing C# logic (JSON parsing, data transformation). Blazor component testing via **bUnit** (v1.27.0+) for interactive component behavior.
- **Moq** (v4.20.0+): Mock dependencies during testing.
- **Visual Studio 2022** (v17.8+) or **Visual Studio Code** + **C# Dev Kit**: Full IDE support for Blazor debugging.
- **dotnet CLI**: Command-line build/test/publish tooling (included in .NET 8 SDK).
```
MyProject.sln
├── MyProject.Reporting/              [Blazor Server App]
│   ├── Pages/
│   │   ├── Index.razor               [Dashboard container]
│   │   └── _Host.cshtml              [Blazor host page]
│   ├── Components/
│   │   ├── DashboardHeader.razor      [Title, subtitle, legend]
│   │   ├── TimelineChart.razor        [SVG timeline with milestones]
│   │   └── HeatmapGrid.razor          [Status heatmap grid]
│   ├── Models/
│   │   └── DashboardModel.cs          [POCO for data.json structure]
│   ├── Services/
│   │   └── DashboardDataService.cs    [JSON file I/O, deserialization]
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css                [Global styles, Grid/Flexbox layout]
│   │   ├── data/
│   │   │   └── data.json              [Project metrics, timeline events]
│   │   └── js/
│   │       └── interop.js             [Optional JS interop for charts]
│   ├── appsettings.json               [Dev config, file paths]
│   └── Program.cs                     [Blazor Server configuration]
├── MyProject.Reporting.Tests/         [xUnit test project]
│   ├── DashboardDataServiceTests.cs
│   └── DashboardModelTests.cs
└── MyProject.sln
```
- **Blazor Pages** as containers for layout structure.
- **Blazor Components** (`*.razor`) for reusable visual sections:
- `DashboardHeader.razor`: Static title, subtitle, legend.
- `TimelineChart.razor`: Stateful component managing timeline data, rendering SVG or OxyPlot chart.
- `HeatmapGrid.razor`: Stateful component with CSS Grid layout, data-driven cell population.
- `StatusCell.razor`: Reusable child component for individual heatmap cells.
- **Services** for business logic:
- `DashboardDataService`: Reads `data.json`, deserializes to models, caches in memory.
- `StatusColorProvider`: Maps status enum to CSS class/color hex.
- **Startup**: `Program.cs` registers `DashboardDataService` as scoped/singleton.
- **Index.razor Page**: Injects `DashboardDataService`, calls `GetDashboardDataAsync()` in `OnInitializedAsync()`.
- **Components**: Receive deserialized data via `@inject` or `[Parameter]`, render without additional API calls.
- **Refresh**: Implement manual refresh button calling `StateHasChanged()` after reloading JSON.
```json
{
  "project": {
    "name": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform • Privacy Automation Workstream • April 2026",
    "adoBacklogUrl": "https://..."
  },
  "milestones": [
    {
      "id": "M1",
      "title": "Chatbot & MS Role",
      "color": "#0078D4",
      "date": "2026-01-12",
      "type": "checkpoint"
    },
    {
      "id": "PoC",
      "title": "PoC Milestone",
      "date": "2026-03-26",
      "type": "poc"
    }
  ],
  "statusRows": [
    {
      "label": "Shipped",
      "category": "shipped",
      "items": [
        { "month": "Jan", "value": "Feature A" },
        { "month": "Feb", "value": "Feature B" }
      ]
    },
    {
      "label": "In Progress",
      "category": "inprogress",
      "items": [...]
    }
  ],
  "nowMarker": "2026-04-12"
}
```
- **Global stylesheet** (`app.css`):
- CSS Custom Properties (CSS Variables) for colors, fonts, spacing (matches design palette).
- CSS Grid for heatmap layout (grid-template-columns, grid-template-rows).
- Flexbox for header, legend, row containers.
- Responsive breakpoints (optional, for print/mobile if PowerPoint screenshots use scaling).
- **Scoped styles** (within `.razor` components):
- Keep component-specific styles in `<style>` blocks at component level.
- Avoid inline styles; prefer CSS classes for maintainability.
- **Server-Side Rendering (SSR)**: All content rendered server-side by ASP.NET Core; minimal JavaScript on client.
- **Caching**: Load `data.json` once at startup or cache in memory with a short TTL (e.g., 5 min). Add refresh endpoint if real-time updates needed.
- **No Virtualization Required**: Dashboard is a single page; DOM is not large enough to justify virtual scrolling.
- **Interactive Features**: Use Blazor event bindings (`@onclick`, `@onchange`) for filtering, sorting (optional interactivity for executives).
- **Visual Studio 2022 Community Edition**: Free, full Blazor debugging support.
- **Chrome DevTools**: F12 for CSS inspection, responsive design testing.
- **PowerShell / dotnet CLI**: Build, test, publish from command line.
- **Git + GitHub**: Version control for `data.json` and code. Treat design screenshots as artifacts in `/docs/screenshots/`. --- **Document Version**: 1.0 **Date**: 2026-04-12 **Stack**: C# .NET 8 Blazor Server, Local Deployment, No Cloud Services

### Considerations & Risks

- **Not required.** The dashboard is read-only, localhost-only, no sensitive data exposure. If deployment to a shared network is needed, add:
- **Windows Authentication** (integrated, Active Directory): Easiest for enterprise networks. Enable via `appsettings.json`:
  ```csharp
  builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
  ```
- **API Key** (if needed): Simple hardcoded key in `appsettings.json` checked via middleware.
- **File I/O Permissions**: Ensure `data.json` is readable by the app process. No sensitive data in JSON.
- **Output Encoding**: Blazor automatically HTML-encodes output; no XSS risk.
- **HTTPS**: Use self-signed certificate for localhost development. In production, use machine-issued cert or HTTP-only (localhost is trusted).
- **Local Development**: `dotnet run` from command line. Blazor Server runs on `https://localhost:5001` by default.
- **Local Production** (for internal team):
- Option A: **Windows Service** via TopShelf. Runs at startup, survives logoff.
- Option B: **Console Application** with auto-start via Task Scheduler.
- Option C: **Docker Container** (if Windows containers available). Simple `Dockerfile` pulls official .NET 8 base image, copies app, runs Kestrel. Not cloud-deployed; runs locally on Docker Desktop or Docker Server.
- **IP Binding**: Change `appsettings.json` to bind to network interface IP (e.g., `http://192.168.1.100:5000`) for LAN access. Add firewall exceptions if needed.
- **Reverse Proxy** (optional): IIS application with Reverse Proxy URL Rewrite module to map clean URL (e.g., `http://reporting.internal/`) to Blazor app.
- **$0**: Local development, single machine.
- **Low**: Docker on local hardware, Windows Service on existing server.
- **Medium**: Dedicated reporting VM (~$10-50/month on cloud, but NOT recommended per requirements).
- **Blazor Component Lifecycle Complexity**: `OnInitializedAsync()` vs. `OnInitialized()`, re-rendering on parameter changes. Mitigation: Use clear naming, document lifecycle in code comments, test with bUnit.
- **SignalR Connection Overhead**: Blazor Server maintains WebSocket connection per client. Large number of concurrent users (~100+) could stress server. Mitigation: Implement connection timeout, heartbeat tuning, or switch to Blazor WebAssembly if needed (requires separate API).
- **JSON Deserialization Errors**: Malformed `data.json` crashes the app. Mitigation: Validate JSON schema at startup, show error UI if parsing fails, log exceptions.
- **SVG Rendering Performance**: Large timelines with many milestones could cause client-side rendering lag. Mitigation: Limit timeline to 12 months, clip off-screen elements, pre-render static SVG if unchanged.
- **Single-Machine Deployment**: Not horizontally scalable. If usage exceeds 1-2 servers, consider splitting into Blazor WebAssembly (static) + WebAPI (backend). Not an issue for internal dashboards.
- **File I/O as Data Source**: No transactions, concurrency control. If multiple processes write `data.json` simultaneously, race conditions occur. Mitigation: Lock file during writes, or migrate to SQL Server (adds complexity).
- **Blazor Component Model**: Mid-level complexity. Developers familiar with ASP.NET MVC or Razor Pages will have gentle ramp; frontend-only developers need .NET fundamentals.
- **CSS Grid/Flexbox**: Well-documented, no major learning gap.
- **Recommendation**: Have team member prototype first component (TimelineChart) to validate approach before full sprint. | Trade-off | Choice | Rationale | |-----------|--------|-----------| | **Charting Library** | OxyPlot vs. Chart.js | OxyPlot avoids JS interop overhead; Chart.js is heavier but more flexible. Choose OxyPlot for simplicity. | | **Styling** | Tailwind vs. Vanilla CSS | Tailwind adds build step + CDN; vanilla CSS is lighter. Use Tailwind only if team preference. | | **Deployment** | Windows Service vs. Console App | Service is more robust; Console App simpler to debug. Use Service for production, Console for dev. | | **Data Refresh** | Manual Refresh Button vs. Auto-polling | Manual is simpler, matches executive use case (static snapshots). Auto-polling adds complexity. | | **Responsive Design** | Full vs. Fixed Layout | Design mock is 1920x1080 fixed. Add CSS media queries only if executives use tablets/laptops. |
- **data.json availability**: If file is locked or missing, app fails to load. Mitigation: Graceful error handling, fallback to cached data.
- **Kestrel server crash**: No automatic restart. Mitigation: Use Windows Service host, or add health-check endpoint.
- **Real-Time Updates**: Does `data.json` change during dashboard viewing? If yes, implement auto-polling (`Task.Delay()` + refresh) or file watcher (`FileSystemWatcher`). Current design assumes static data per session.
- **Multi-Environment Support** (Dev/Staging/Prod): Should the app read `data.json` from different paths per environment? Recommend `appsettings.Development.json` and `appsettings.Production.json` for file paths.
- **Print/Export**: Do executives need "Export to PDF" functionality? If yes, consider **SelectPdf** (NuGet, commercial, ~$300) or **iTextSharp** (open-source, v5.5.13+) to generate PDF reports from rendered HTML.
- **Interactivity**: Should executives interact with the dashboard (filter by milestone, drill-down into details)? Current design is read-only. Adding filtering requires parameter binding and component state management.
- **Data Source Evolution**: Will `data.json` be hand-edited, or generated by an external system (Azure DevOps API, Jira, etc.)? Current design assumes manual JSON. If automated, plan for API client or middleware.
- **Mobile/Tablet Viewing**: Are executives viewing on mobile devices, or primarily desktop for PowerPoint export? Design mock is 1920x1080; mobile responsiveness is optional.

### Detailed Analysis

# Research.md: Executive Reporting Dashboard Technology Stack

## Executive Summary

This project requires a lightweight, screenshot-friendly executive dashboard built with C# .NET 8 and Blazor Server. The solution reads project metadata and status from a JSON configuration file and renders a timeline-based reporting view with status heatmap visualization. Given the simplicity requirements (no authentication, local-only deployment, static data binding), Blazor Server with built-in templating and charting libraries is ideally suited. The minimal infrastructure footprint and rapid development velocity make this stack optimal for iterative refinement toward PowerPoint-ready output. Primary risks are Blazor component lifecycle complexity and ensuring visual fidelity to the design mock.

## Key Findings

- Blazor Server eliminates the need for separate frontend frameworks (React, Vue) while maintaining full C# type safety and code reuse; ideal for dashboard work with no authentication overhead.
- SVG-based charting (using a lightweight library like OxyPlot or Chart.js bridge) is preferred over heavy canvas libraries because the design references SVG for timeline visualization.
- CSS Grid and Flexbox are native browser technologies and require no additional libraries; the OriginalDesignConcept.html already demonstrates the exact layout approach needed.
- JSON deserialization in .NET 8 is built-in via `System.Text.Json`; no additional serialization libraries required.
- Blazor's component model maps naturally to the dashboard's visual sections (Header, Timeline, Heatmap), enabling clean separation of concerns.
- Local file I/O via `File.ReadAllTextAsync()` is sufficient for reading data.json; no database infrastructure needed.
- Static site generation is not required; Blazor Server handles dynamic rendering server-side with minimal client-side JavaScript.

## Recommended Technology Stack

### Frontend Layer
- **Blazor Server** (included in .NET 8 SDK, v8.0.0+): Server-side C# rendering with SignalR WebSocket communication. No SPA framework needed. Use default `.razor` component format.
- **Tailwind CSS** (v3.4.0+) or **native CSS**: The design mock uses simple CSS Grid/Flexbox. Tailwind is optional but recommended for utility-first styling consistency. Alternatively, embed CSS directly in Blazor component `.razor` files or a single `app.css`.
- **Chart.js** (v4.4.0+) via Interop or **OxyPlot** (v2.1.2+, NuGet): For timeline and heatmap visualization. OxyPlot is C#-native and Blazor-friendly. Chart.js requires JavaScript interop.
  - **Recommendation**: Use OxyPlot for minimal JS dependency overhead; it generates SVG output matching the design aesthetic.
- **FontAwesome** (v6.5.0+, free CDN or NuGet): Optional icon library for legend markers and status indicators. The design uses diamond/circle/line shapes that can be replicated with pure CSS or inline SVG.

### Backend Layer
- **.NET 8 ASP.NET Core** (v8.0.0+): Built into Blazor Server. No additional web framework needed.
- **System.Text.Json** (included in .NET 8): Deserialize `data.json` into POCO models. No external JSON library required.
- **ILogger** (built-in, included in .NET Generic Host): Minimal logging for debugging file I/O and data parsing.

### Data Layer
- **Local JSON file** (`data.json`): Single source of truth. No database. Use `File.ReadAllTextAsync()` from `System.IO` (built-in).
- **POCO Models** (C# classes): Strongly-typed representation of JSON structure. Auto-generated via Visual Studio "Paste JSON as Classes" or manual POCO definition.

### Infrastructure & Deployment
- **Local HTTP Server**: Blazor Server runs on `https://localhost:5001` by default (or HTTP on `localhost:5000`). No cloud infrastructure required.
- **Kestrel** (built-in ASP.NET Core web server): Default HTTP server. No IIS or external proxy required.
- **Windows Service or Console App**: For long-term local deployment, wrap the Blazor app as a Windows Service using `TopShelf` (v4.4.1+, NuGet) or run as a background console app.
- **Screenshot Automation**: Use Puppeteer Sharp (v10.0.0+) or Selenium (v4.15.0+) for headless browser screenshots if PowerPoint deck generation is automated. For manual screenshots, Chrome DevTools or `Win+Shift+S` suffice.

### Testing & Development
- **xUnit** (v2.7.0+) or **NUnit** (v4.0.0+): Unit testing C# logic (JSON parsing, data transformation). Blazor component testing via **bUnit** (v1.27.0+) for interactive component behavior.
- **Moq** (v4.20.0+): Mock dependencies during testing.
- **Visual Studio 2022** (v17.8+) or **Visual Studio Code** + **C# Dev Kit**: Full IDE support for Blazor debugging.
- **dotnet CLI**: Command-line build/test/publish tooling (included in .NET 8 SDK).

### Project Structure (.sln)
```
MyProject.sln
├── MyProject.Reporting/              [Blazor Server App]
│   ├── Pages/
│   │   ├── Index.razor               [Dashboard container]
│   │   └── _Host.cshtml              [Blazor host page]
│   ├── Components/
│   │   ├── DashboardHeader.razor      [Title, subtitle, legend]
│   │   ├── TimelineChart.razor        [SVG timeline with milestones]
│   │   └── HeatmapGrid.razor          [Status heatmap grid]
│   ├── Models/
│   │   └── DashboardModel.cs          [POCO for data.json structure]
│   ├── Services/
│   │   └── DashboardDataService.cs    [JSON file I/O, deserialization]
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css                [Global styles, Grid/Flexbox layout]
│   │   ├── data/
│   │   │   └── data.json              [Project metrics, timeline events]
│   │   └── js/
│   │       └── interop.js             [Optional JS interop for charts]
│   ├── appsettings.json               [Dev config, file paths]
│   └── Program.cs                     [Blazor Server configuration]
├── MyProject.Reporting.Tests/         [xUnit test project]
│   ├── DashboardDataServiceTests.cs
│   └── DashboardModelTests.cs
└── MyProject.sln
```

## Architecture Recommendations

### Design Pattern: Component Composition
- **Blazor Pages** as containers for layout structure.
- **Blazor Components** (`*.razor`) for reusable visual sections:
  - `DashboardHeader.razor`: Static title, subtitle, legend.
  - `TimelineChart.razor`: Stateful component managing timeline data, rendering SVG or OxyPlot chart.
  - `HeatmapGrid.razor`: Stateful component with CSS Grid layout, data-driven cell population.
  - `StatusCell.razor`: Reusable child component for individual heatmap cells.
- **Services** for business logic:
  - `DashboardDataService`: Reads `data.json`, deserializes to models, caches in memory.
  - `StatusColorProvider`: Maps status enum to CSS class/color hex.

### Data Flow
1. **Startup**: `Program.cs` registers `DashboardDataService` as scoped/singleton.
2. **Index.razor Page**: Injects `DashboardDataService`, calls `GetDashboardDataAsync()` in `OnInitializedAsync()`.
3. **Components**: Receive deserialized data via `@inject` or `[Parameter]`, render without additional API calls.
4. **Refresh**: Implement manual refresh button calling `StateHasChanged()` after reloading JSON.

### Data Model Structure (JSON)
```json
{
  "project": {
    "name": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform • Privacy Automation Workstream • April 2026",
    "adoBacklogUrl": "https://..."
  },
  "milestones": [
    {
      "id": "M1",
      "title": "Chatbot & MS Role",
      "color": "#0078D4",
      "date": "2026-01-12",
      "type": "checkpoint"
    },
    {
      "id": "PoC",
      "title": "PoC Milestone",
      "date": "2026-03-26",
      "type": "poc"
    }
  ],
  "statusRows": [
    {
      "label": "Shipped",
      "category": "shipped",
      "items": [
        { "month": "Jan", "value": "Feature A" },
        { "month": "Feb", "value": "Feature B" }
      ]
    },
    {
      "label": "In Progress",
      "category": "inprogress",
      "items": [...]
    }
  ],
  "nowMarker": "2026-04-12"
}
```

### CSS Architecture
- **Global stylesheet** (`app.css`):
  - CSS Custom Properties (CSS Variables) for colors, fonts, spacing (matches design palette).
  - CSS Grid for heatmap layout (grid-template-columns, grid-template-rows).
  - Flexbox for header, legend, row containers.
  - Responsive breakpoints (optional, for print/mobile if PowerPoint screenshots use scaling).
- **Scoped styles** (within `.razor` components):
  - Keep component-specific styles in `<style>` blocks at component level.
  - Avoid inline styles; prefer CSS classes for maintainability.

### Rendering & Performance
- **Server-Side Rendering (SSR)**: All content rendered server-side by ASP.NET Core; minimal JavaScript on client.
- **Caching**: Load `data.json` once at startup or cache in memory with a short TTL (e.g., 5 min). Add refresh endpoint if real-time updates needed.
- **No Virtualization Required**: Dashboard is a single page; DOM is not large enough to justify virtual scrolling.
- **Interactive Features**: Use Blazor event bindings (`@onclick`, `@onchange`) for filtering, sorting (optional interactivity for executives).

## Security & Infrastructure

### Authentication & Authorization
**Not required.** The dashboard is read-only, localhost-only, no sensitive data exposure. If deployment to a shared network is needed, add:
- **Windows Authentication** (integrated, Active Directory): Easiest for enterprise networks. Enable via `appsettings.json`:
  ```csharp
  builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
  ```
- **API Key** (if needed): Simple hardcoded key in `appsettings.json` checked via middleware.

### Data Protection
- **File I/O Permissions**: Ensure `data.json` is readable by the app process. No sensitive data in JSON.
- **Output Encoding**: Blazor automatically HTML-encodes output; no XSS risk.
- **HTTPS**: Use self-signed certificate for localhost development. In production, use machine-issued cert or HTTP-only (localhost is trusted).

### Hosting & Deployment
- **Local Development**: `dotnet run` from command line. Blazor Server runs on `https://localhost:5001` by default.
- **Local Production** (for internal team):
  - Option A: **Windows Service** via TopShelf. Runs at startup, survives logoff.
  - Option B: **Console Application** with auto-start via Task Scheduler.
  - Option C: **Docker Container** (if Windows containers available). Simple `Dockerfile` pulls official .NET 8 base image, copies app, runs Kestrel. Not cloud-deployed; runs locally on Docker Desktop or Docker Server.
- **IP Binding**: Change `appsettings.json` to bind to network interface IP (e.g., `http://192.168.1.100:5000`) for LAN access. Add firewall exceptions if needed.
- **Reverse Proxy** (optional): IIS application with Reverse Proxy URL Rewrite module to map clean URL (e.g., `http://reporting.internal/`) to Blazor app.

### Infrastructure Costs
- **$0**: Local development, single machine.
- **Low**: Docker on local hardware, Windows Service on existing server.
- **Medium**: Dedicated reporting VM (~$10-50/month on cloud, but NOT recommended per requirements).

## Risks & Trade-offs

### Technical Risks
1. **Blazor Component Lifecycle Complexity**: `OnInitializedAsync()` vs. `OnInitialized()`, re-rendering on parameter changes. Mitigation: Use clear naming, document lifecycle in code comments, test with bUnit.
2. **SignalR Connection Overhead**: Blazor Server maintains WebSocket connection per client. Large number of concurrent users (~100+) could stress server. Mitigation: Implement connection timeout, heartbeat tuning, or switch to Blazor WebAssembly if needed (requires separate API).
3. **JSON Deserialization Errors**: Malformed `data.json` crashes the app. Mitigation: Validate JSON schema at startup, show error UI if parsing fails, log exceptions.
4. **SVG Rendering Performance**: Large timelines with many milestones could cause client-side rendering lag. Mitigation: Limit timeline to 12 months, clip off-screen elements, pre-render static SVG if unchanged.

### Scalability Bottlenecks
- **Single-Machine Deployment**: Not horizontally scalable. If usage exceeds 1-2 servers, consider splitting into Blazor WebAssembly (static) + WebAPI (backend). Not an issue for internal dashboards.
- **File I/O as Data Source**: No transactions, concurrency control. If multiple processes write `data.json` simultaneously, race conditions occur. Mitigation: Lock file during writes, or migrate to SQL Server (adds complexity).

### Skill Gap & Learning Curve
- **Blazor Component Model**: Mid-level complexity. Developers familiar with ASP.NET MVC or Razor Pages will have gentle ramp; frontend-only developers need .NET fundamentals.
- **CSS Grid/Flexbox**: Well-documented, no major learning gap.
- **Recommendation**: Have team member prototype first component (TimelineChart) to validate approach before full sprint.

### Trade-offs
| Trade-off | Choice | Rationale |
|-----------|--------|-----------|
| **Charting Library** | OxyPlot vs. Chart.js | OxyPlot avoids JS interop overhead; Chart.js is heavier but more flexible. Choose OxyPlot for simplicity. |
| **Styling** | Tailwind vs. Vanilla CSS | Tailwind adds build step + CDN; vanilla CSS is lighter. Use Tailwind only if team preference. |
| **Deployment** | Windows Service vs. Console App | Service is more robust; Console App simpler to debug. Use Service for production, Console for dev. |
| **Data Refresh** | Manual Refresh Button vs. Auto-polling | Manual is simpler, matches executive use case (static snapshots). Auto-polling adds complexity. |
| **Responsive Design** | Full vs. Fixed Layout | Design mock is 1920x1080 fixed. Add CSS media queries only if executives use tablets/laptops. |

### Single Points of Failure
- **data.json availability**: If file is locked or missing, app fails to load. Mitigation: Graceful error handling, fallback to cached data.
- **Kestrel server crash**: No automatic restart. Mitigation: Use Windows Service host, or add health-check endpoint.

## Open Questions

1. **Real-Time Updates**: Does `data.json` change during dashboard viewing? If yes, implement auto-polling (`Task.Delay()` + refresh) or file watcher (`FileSystemWatcher`). Current design assumes static data per session.
2. **Multi-Environment Support** (Dev/Staging/Prod): Should the app read `data.json` from different paths per environment? Recommend `appsettings.Development.json` and `appsettings.Production.json` for file paths.
3. **Print/Export**: Do executives need "Export to PDF" functionality? If yes, consider **SelectPdf** (NuGet, commercial, ~$300) or **iTextSharp** (open-source, v5.5.13+) to generate PDF reports from rendered HTML.
4. **Interactivity**: Should executives interact with the dashboard (filter by milestone, drill-down into details)? Current design is read-only. Adding filtering requires parameter binding and component state management.
5. **Data Source Evolution**: Will `data.json` be hand-edited, or generated by an external system (Azure DevOps API, Jira, etc.)? Current design assumes manual JSON. If automated, plan for API client or middleware.
6. **Mobile/Tablet Viewing**: Are executives viewing on mobile devices, or primarily desktop for PowerPoint export? Design mock is 1920x1080; mobile responsiveness is optional.

## Implementation Recommendations

### Phasing & MVP Scope

**Phase 1 (Week 1): Core Rendering**
- Set up Blazor Server project in Visual Studio.
- Create POCO models matching `data.json` structure.
- Implement `DashboardDataService` with `GetDashboardDataAsync()` (hardcoded `data.json` path).
- Build `DashboardHeader.razor` component (title, subtitle, legend).
- Build `HeatmapGrid.razor` component with CSS Grid layout, static sample data.
- Verify visual layout matches design mock (1920x1080 viewport).
- **Deliverable**: Blazor app running on `localhost:5001`, rendering header + heatmap with dummy data.

**Phase 2 (Week 2): Dynamic Data & Timeline**
- Modify `data.json` to include sample project data (4 status rows, 12 months, 3 milestones).
- Wire `DashboardDataService` to `DashboardDataService` and render real data in `HeatmapGrid.razor`.
- Build `TimelineChart.razor` with OxyPlot or SVG rendering of timeline + milestones.
- Implement color mapping (status category → CSS class).
- **Deliverable**: Dashboard reads and renders full `data.json`, timeline visual matches design.

**Phase 3 (Week 3): Polish & Deployment**
- Add CSS refinements (fonts, spacing, colors) to match design pixel-perfectly.
- Implement refresh button + error handling for malformed `data.json`.
- Add optional features (filter by category, legend click-to-highlight).
- Deploy as Windows Service or Docker container.
- **Deliverable**: Production-ready dashboard, ready for executive PowerPoint screenshots.

### Quick Wins (High-Value, Low-Effort)
1. **Copy design CSS directly**: Extract styles from `OriginalDesignConcept.html` into `app.css`. Saves design iteration time.
2. **Hardcode sample data**: Populate `data.json` with 2-3 sample projects. Developers and executives can verify layout without ongoing data integration.
3. **Responsive print styles**: Add `@media print` CSS to hide UI chrome (buttons, legends) for clean PowerPoint screenshots.
4. **Dark mode toggle** (optional): Add CSS variables for light/dark theme. Easy UI win for executives.

### Prototyping & Validation
- **Priority 1**: Build static HTML mock in Phase 1 (DashboardHeader + HeatmapGrid) to validate visual fidelity before committing to Blazor. Use `OriginalDesignConcept.html` as template.
- **Priority 2**: Prototype OxyPlot timeline rendering (Phase 2) to ensure SVG output matches design. If OxyPlot output is unsatisfactory, fall back to custom SVG generation or Chart.js.
- **Priority 3**: Test Blazor component refresh performance with large heatmaps (100+ cells) to identify rendering bottlenecks early.

### Recommended Tools & Workflow
- **Visual Studio 2022 Community Edition**: Free, full Blazor debugging support.
- **Chrome DevTools**: F12 for CSS inspection, responsive design testing.
- **PowerShell / dotnet CLI**: Build, test, publish from command line.
- **Git + GitHub**: Version control for `data.json` and code. Treat design screenshots as artifacts in `/docs/screenshots/`.

---

**Document Version**: 1.0  
**Date**: 2026-04-12  
**Stack**: C# .NET 8 Blazor Server, Local Deployment, No Cloud Services

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/f6995b6cb1ef719b7af766c1807a7c380b4d16fb/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
