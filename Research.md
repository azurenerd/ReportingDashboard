# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 15:35 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard reads project data from a `data.json` configuration file and renders a visually clean, screenshot-friendly view of project milestones, shipped features, in-progress work, and carry-over items. Given the deliberately simple scope—no auth, no enterprise security, no multi-user concerns—the architecture should be minimal: a single Blazor Server project with a flat component structure, JSON file deserialization via `System.Text.Json`, and CSS-driven layout optimized for screen capture into PowerPoint decks. The primary recommendation is to keep the dependency footprint near zero, leveraging built-in .NET 8 capabilities and hand-crafted CSS rather than heavy UI frameworks.

### Key Findings

- Blazor Server in .NET 8 is fully capable of rendering a static-feeling single-page dashboard with real-time data binding from a local JSON file, requiring zero JavaScript.
- `System.Text.Json` (built into .NET 8) is the optimal JSON parser—no need for Newtonsoft.Json for this use case.
- No UI component library is strictly necessary; the original HTML design can be translated directly into Blazor `.razor` components with scoped CSS, preserving the clean screenshot aesthetic.
- A timeline/milestone visualization is achievable with pure CSS (flexbox/grid) and does not require a charting library, keeping the output pixel-perfect for screenshots.
- If richer charts are desired (e.g., burndown, progress bars with animations), **Radzen.Blazor** (free, MIT-licensed) or **MudBlazor** (MIT-licensed) offer lightweight component options without commercial licensing concerns.
- .NET 8's `IFileSystemWatcher` pattern or simple polling can enable live-reload of `data.json` changes during editing, providing a smooth authoring workflow.
- The project should be a single `.sln` with one `.csproj`—no microservices, no API layer, no database. Maximum simplicity.
- Blazor Server's SignalR circuit is irrelevant for a single-user local app but imposes no overhead; no configuration changes needed.
- Screenshot fidelity is a first-class requirement: avoid component libraries that inject unpredictable CSS, shadow DOM, or dynamic sizing that could shift between renders.
- **Scaffold project** — `dotnet new blazorserver-empty -n ReportingDashboard`
- **Define data models** — C# records matching the JSON structure
- **Create `DashboardDataService`** — Read and deserialize `data.json` at startup
- **Build `Dashboard.razor`** — Single page with all sections inline initially
- **Port CSS from original HTML** — Translate the existing design's styles into `app.css`
- **Create sample `data.json`** — Fictional project with realistic milestone data
- **Verify screenshot quality** — Capture at 1280px width, confirm PowerPoint compatibility
- **Extract child components** — `MilestoneTimeline`, `StatusCard`, `MetricBadge`
- **Add `FileSystemWatcher`** — Auto-refresh when `data.json` is saved
- **Add print/screenshot CSS** — `@media print` rules, optional `?clean=true` query parameter that hides browser elements
- **Status color system** — CSS custom properties: `--status-shipped`, `--status-in-progress`, `--status-carried-over`, `--status-at-risk`
- **Improve timeline** — Add "today" marker, milestone status indicators (complete/in-progress/upcoming)
- **Multiple project support** — Dropdown or route parameter to switch `data.json` files
- **Simple edit form** — Blazor form to modify `data.json` fields without hand-editing
- **PDF export** — Use browser print-to-PDF (no library needed given the fixed layout)
- **Historical comparison** — Load previous month's JSON to show delta indicators
- **Immediate value:** A working page with the fictional project data demonstrates the concept in under 4 hours.
- **`dotnet watch` workflow:** Edit `data.json` → save → dashboard updates instantly. Demonstrate this loop to stakeholders.
- **Single-file publish:** Distribute as one `.exe` that anyone can double-click to see their project dashboard locally.
- **Prototype the timeline component first** — It's the most visually complex element and the hardest to get right for screenshots. Use pure CSS with `display: flex` and positioned markers before considering any library.
- **Test screenshot quality early** — Take a screenshot on Day 1 and paste it into PowerPoint. Identify font rendering, color, and sizing issues before building out all sections.
- **Validate `data.json` schema with real project data** — Ask a project manager to fill in real milestone data to ensure the schema captures what they actually track.
- ---
- ```json
- {
- "project": {
- "name": "Project Phoenix",
- "executive": "Jane Smith, VP Engineering",
- "status": "On Track",
- "reportDate": "2026-04-10",
- "reportingPeriod": "March 2026"
- },
- "milestones": [
- {
- "title": "Architecture Complete",
- "date": "2026-01-15",
- "status": "Completed",
- "description": "System design finalized and approved"
- },
- {
- "title": "Beta Launch",
- "date": "2026-04-01",
- "status": "In Progress",
- "description": "Internal beta with pilot customers"
- }
- ],
- "shipped": [
- {
- "title": "User Authentication Module",
- "description": "OAuth2 + MFA integration complete",
- "category": "Security",
- "priority": "P0"
- }
- ],
- "inProgress": [
- {
- "title": "Dashboard Analytics",
- "description": "Real-time metrics pipeline",
- "category": "Core Feature",
- "priority": "P0",
- "percentComplete": 65
- }
- ],
- "carriedOver": [
- {
- "title": "Mobile Responsive Layout",
- "description": "Deferred from February due to API dependency",
- "category": "UX",
- "priority": "P1",
- "originalTarget": "February 2026",
- "reason": "Blocked by API v2 migration"
- }
- ],
- "metrics": [
- { "label": "Sprint Velocity", "value": "42 pts", "trend": "up" },
- { "label": "Bug Escape Rate", "value": "2.1%", "trend": "down" },
- { "label": "Team Capacity", "value": "85%", "trend": "stable" }
- ]
- }
- ```
- | Package | Version | License | Use Case |
- |---------|---------|---------|----------|
- | `MudBlazor` | 7.x | MIT | Only if timeline/progress components needed |
- | `System.Text.Json` | Built-in | MIT | JSON deserialization (no install needed) |
- | `Microsoft.Extensions.FileProviders.Physical` | Built-in | MIT | File watching (no install needed) |
- | `bUnit` | 1.25.x | MIT | Component testing (dev dependency only) |
- | `xUnit` | 2.7.x | Apache-2.0 | Unit testing (dev dependency only) |

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes |
- |-------|-----------|---------|-------|
- | **Runtime** | .NET 8 SDK | 8.0.x (LTS) | Long-term support until Nov 2026 |
- | **Web Framework** | Blazor Server | Built into .NET 8 | No separate package needed |
- | **Project Template** | `blazorserver-empty` | .NET 8 | Minimal template, no sample pages |
- | Component | Recommendation | Version | Rationale |
- |-----------|---------------|---------|-----------|
- | **CSS Framework** | Custom CSS (hand-rolled) | N/A | Full control over screenshot appearance; no class-name bloat |
- | **CSS Methodology** | Scoped CSS (`.razor.css` files) | Built-in | Component-level isolation, no global leakage |
- | **Layout System** | CSS Grid + Flexbox | Native | Timeline row, card grid, status sections |
- | **Icons** | Inline SVG or CSS-only indicators | N/A | Colored circles/badges for status; zero dependencies |
- | **Fonts** | System font stack or single Google Font embed | N/A | `Segoe UI` for Windows consistency with PowerPoint |
- | **Optional: Component Lib** | MudBlazor | 7.x | Only if progress bars, tooltips, or chips are desired; MIT license |
- | Component | Recommendation | Version | Rationale |
- |-----------|---------------|---------|-----------|
- | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | High performance, source-gen support, zero dependencies |
- | **Data Models** | C# records | Language feature | Immutable, concise, perfect for config deserialization |
- | **File Watching** | `FileSystemWatcher` | `System.IO` built-in | Auto-reload dashboard when `data.json` is edited |
- | **Data Storage** | `data.json` flat file | N/A | Single JSON file in project root or `wwwroot/data/` |
- | Tool | Version | Purpose |
- |------|---------|---------|
- | **SDK** | .NET 8.0.x | Build, run, publish |
- | **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Full Blazor support |
- | **Hot Reload** | Built into `dotnet watch` | Instant CSS/Razor changes |
- | **Package Manager** | NuGet | Only if adding optional libraries |
- | Tool | Version | Purpose |
- |------|---------|---------|
- | **Unit Testing** | xUnit | 2.7.x | Test JSON deserialization, data model logic |
- | **Blazor Component Testing** | bUnit | 1.25.x | Test component rendering if complexity grows |
- | **Assertions** | FluentAssertions | 6.12.x | Readable test assertions |
- > **Note:** Given the project's simplicity (single page, read-only data, no user interaction beyond viewing), testing is optional for MVP but recommended for the JSON parsing layer.
- ```
- ReportingDashboard.sln
- └── ReportingDashboard/
- ├── ReportingDashboard.csproj
- ├── Program.cs                     # Minimal host builder
- ├── Models/
- │   └── DashboardData.cs           # C# records for JSON shape
- ├── Services/
- │   └── DashboardDataService.cs    # Reads & watches data.json
- ├── Components/
- │   ├── App.razor                  # Root component
- │   ├── Pages/
- │   │   └── Dashboard.razor        # Single page
- │   └── Shared/
- │       ├── MilestoneTimeline.razor # Horizontal timeline strip
- │       ├── StatusCard.razor        # Shipped / In Progress / Carried Over
- │       ├── ProjectHeader.razor     # Project name, dates, health
- │       └── MetricBadge.razor       # KPI indicators
- ├── wwwroot/
- │   ├── css/
- │   │   └── app.css                # Global styles, print styles
- │   └── data/
- │       └── data.json              # Project data file
- └── Properties/
- └── launchSettings.json
- ```
- This project does not need CQRS, MediatR, Clean Architecture, or any layered pattern. The appropriate pattern is:
- **`DashboardDataService`** — A singleton service registered in DI that reads `data.json` on startup and exposes a `DashboardData` property. Optionally uses `FileSystemWatcher` to reload on file change.
- **Component Tree** — `Dashboard.razor` injects the service, passes data down to child components via parameters. No state management library needed.
- **No API layer** — Components read directly from the service. No REST endpoints, no controllers, no middleware.
- ```csharp
- public record DashboardData(
- ProjectInfo Project,
- List<Milestone> Milestones,
- List<WorkItem> Shipped,
- List<WorkItem> InProgress,
- List<WorkItem> CarriedOver,
- List<KeyMetric> Metrics
- );
- public record ProjectInfo(string Name, string Executive, string Status,
- string ReportDate, string ReportingPeriod);
- public record Milestone(string Title, string Date, string Status, string Description);
- public record WorkItem(string Title, string Description, string Category,
- string Priority, string Status);
- public record KeyMetric(string Label, string Value, string Trend);
- ```
- ```
- data.json  →  DashboardDataService (singleton, file watcher)
- ↓
- Dashboard.razor (page)
- /        |          \
- Timeline    StatusCards    Metrics
- ```
- Use `@media print` styles for clean screenshot output.
- Fixed-width layout (e.g., 1280px) rather than fluid responsive—ensures consistent screenshots.
- Avoid animations and transitions that could be mid-frame during capture.
- Use explicit `background-color` on all elements (transparent backgrounds screenshot poorly).
- Consider a `?print=true` query parameter that hides browser chrome elements and forces the clean layout.

### Considerations & Risks

- `data.json` contains project metadata, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` outside `wwwroot/` and read it from a configurable file path via `appsettings.json`.
- | Aspect | Recommendation |
- |--------|---------------|
- | **Runtime** | Local `dotnet run` or `dotnet watch` |
- | **Port** | `https://localhost:5001` or `http://localhost:5000` |
- | **Distribution** | Self-contained publish: `dotnet publish -c Release -r win-x64 --self-contained` |
- | **Executable** | Single-file publish for easy sharing: add `<PublishSingleFile>true</PublishSingleFile>` |
- | **No Docker** | Unnecessary for a local single-user tool |
- | **No Reverse Proxy** | Kestrel direct is sufficient |
- | **No HTTPS in Dev** | Use `http://` to avoid certificate warnings during screenshots |
- ```bash
- dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
- ```
- This produces a single `.exe` that any team member can run without installing .NET.
- | Risk | Severity | Mitigation |
- |------|----------|------------|
- | **Blazor Server overhead for a static page** | Low | Blazor Server's SignalR circuit adds ~50KB overhead but is negligible for local use. Alternatively, consider Blazor Static SSR (new in .NET 8) which renders pure HTML with no circuit. |
- | **JSON schema drift** | Medium | Define C# records as the schema contract. Add a startup validation step that logs warnings for missing/extra fields. |
- | **Screenshot inconsistency across monitors** | Medium | Use fixed-width layout (1280px). Provide a dedicated `/screenshot` route with `overflow: hidden` and no scrollbar. |
- | **FileSystemWatcher reliability on Windows** | Low | Known to occasionally miss events. Implement a fallback polling mechanism (every 5 seconds) as backup. |
- | **Over-engineering temptation** | High | This is a screenshot tool, not a SaaS product. Resist adding databases, caching layers, or component libraries unless a specific visual element demands it. |
- | Decision | Trade-off | Rationale |
- |----------|-----------|-----------|
- | No component library | Must hand-code progress bars, timelines | Full visual control for screenshots; avoids theme conflicts |
- | No database | Data lives in a flat file | Single JSON file is trivially editable; no migration headaches |
- | Blazor Server over Static SSR | Maintains SignalR circuit unnecessarily | Simpler project template; hot reload works better; trivial overhead locally |
- | No API layer | Components coupled to data service | Appropriate for single-page, single-user app; reduces boilerplate by ~60% |
- | Custom CSS over Tailwind/Bootstrap | More CSS to write initially | No build pipeline for CSS; no class-name noise in Razor markup |
- Any C# developer can be productive immediately; Blazor `.razor` syntax is intuitive.
- CSS Grid/Flexbox knowledge needed for timeline component—provide a reference implementation.
- `System.Text.Json` deserialization has quirks (e.g., case sensitivity defaults)—use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.
- | # | Question | Impact | Recommended Default |
- |---|----------|--------|-------------------|
- | 1 | What screen resolution should screenshots target? | Layout dimensions | 1280×900 (standard 16:9 PowerPoint slide at 96 DPI) |
- | 2 | Should the timeline be horizontal (top of page) or vertical (sidebar)? | Component layout | Horizontal across top, matching the original HTML design |
- | 3 | How many months of data should one report cover? | Data model scope | Single reporting period (1 month) with carry-over from previous |
- | 4 | Should the tool support multiple projects or strictly one? | `data.json` schema | Single project per `data.json`; switch projects by swapping files |
- | 5 | Is dark mode needed or strictly light theme? | CSS scope | Light theme only (better for PowerPoint embedding) |
- | 6 | Should color-coding follow a specific corporate palette? | CSS variables | Use the palette from the original HTML design; expose as CSS custom properties for easy override |
- | 7 | Will users edit `data.json` by hand or should there be an edit UI? | Feature scope | Hand-edit for MVP; consider a simple edit form in Phase 2 |

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard reads project data from a `data.json` configuration file and renders a visually clean, screenshot-friendly view of project milestones, shipped features, in-progress work, and carry-over items. Given the deliberately simple scope—no auth, no enterprise security, no multi-user concerns—the architecture should be minimal: a single Blazor Server project with a flat component structure, JSON file deserialization via `System.Text.Json`, and CSS-driven layout optimized for screen capture into PowerPoint decks. The primary recommendation is to keep the dependency footprint near zero, leveraging built-in .NET 8 capabilities and hand-crafted CSS rather than heavy UI frameworks.

## 2. Key Findings

- Blazor Server in .NET 8 is fully capable of rendering a static-feeling single-page dashboard with real-time data binding from a local JSON file, requiring zero JavaScript.
- `System.Text.Json` (built into .NET 8) is the optimal JSON parser—no need for Newtonsoft.Json for this use case.
- No UI component library is strictly necessary; the original HTML design can be translated directly into Blazor `.razor` components with scoped CSS, preserving the clean screenshot aesthetic.
- A timeline/milestone visualization is achievable with pure CSS (flexbox/grid) and does not require a charting library, keeping the output pixel-perfect for screenshots.
- If richer charts are desired (e.g., burndown, progress bars with animations), **Radzen.Blazor** (free, MIT-licensed) or **MudBlazor** (MIT-licensed) offer lightweight component options without commercial licensing concerns.
- .NET 8's `IFileSystemWatcher` pattern or simple polling can enable live-reload of `data.json` changes during editing, providing a smooth authoring workflow.
- The project should be a single `.sln` with one `.csproj`—no microservices, no API layer, no database. Maximum simplicity.
- Blazor Server's SignalR circuit is irrelevant for a single-user local app but imposes no overhead; no configuration changes needed.
- Screenshot fidelity is a first-class requirement: avoid component libraries that inject unpredictable CSS, shadow DOM, or dynamic sizing that could shift between renders.

## 3. Recommended Technology Stack

### Runtime & Framework

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Runtime** | .NET 8 SDK | 8.0.x (LTS) | Long-term support until Nov 2026 |
| **Web Framework** | Blazor Server | Built into .NET 8 | No separate package needed |
| **Project Template** | `blazorserver-empty` | .NET 8 | Minimal template, no sample pages |

### Frontend / UI

| Component | Recommendation | Version | Rationale |
|-----------|---------------|---------|-----------|
| **CSS Framework** | Custom CSS (hand-rolled) | N/A | Full control over screenshot appearance; no class-name bloat |
| **CSS Methodology** | Scoped CSS (`.razor.css` files) | Built-in | Component-level isolation, no global leakage |
| **Layout System** | CSS Grid + Flexbox | Native | Timeline row, card grid, status sections |
| **Icons** | Inline SVG or CSS-only indicators | N/A | Colored circles/badges for status; zero dependencies |
| **Fonts** | System font stack or single Google Font embed | N/A | `Segoe UI` for Windows consistency with PowerPoint |
| **Optional: Component Lib** | MudBlazor | 7.x | Only if progress bars, tooltips, or chips are desired; MIT license |

**Why no heavy UI library by default:** The original HTML design is intentionally simple—clean cards, status badges, a horizontal timeline. Introducing MudBlazor or Radzen adds ~2MB of CSS/JS and opinionated theming that fights screenshot consistency. Start with raw CSS; add MudBlazor only if specific components (e.g., `MudProgressLinear`, `MudTimeline`) save significant effort.

### Data Layer

| Component | Recommendation | Version | Rationale |
|-----------|---------------|---------|-----------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | High performance, source-gen support, zero dependencies |
| **Data Models** | C# records | Language feature | Immutable, concise, perfect for config deserialization |
| **File Watching** | `FileSystemWatcher` | `System.IO` built-in | Auto-reload dashboard when `data.json` is edited |
| **Data Storage** | `data.json` flat file | N/A | Single JSON file in project root or `wwwroot/data/` |

### Build & Tooling

| Tool | Version | Purpose |
|------|---------|---------|
| **SDK** | .NET 8.0.x | Build, run, publish |
| **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Full Blazor support |
| **Hot Reload** | Built into `dotnet watch` | Instant CSS/Razor changes |
| **Package Manager** | NuGet | Only if adding optional libraries |

### Testing (Lightweight)

| Tool | Version | Purpose |
|------|---------|---------|
| **Unit Testing** | xUnit | 2.7.x | Test JSON deserialization, data model logic |
| **Blazor Component Testing** | bUnit | 1.25.x | Test component rendering if complexity grows |
| **Assertions** | FluentAssertions | 6.12.x | Readable test assertions |

> **Note:** Given the project's simplicity (single page, read-only data, no user interaction beyond viewing), testing is optional for MVP but recommended for the JSON parsing layer.

## 4. Architecture Recommendations

### Project Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                     # Minimal host builder
    ├── Models/
    │   └── DashboardData.cs           # C# records for JSON shape
    ├── Services/
    │   └── DashboardDataService.cs    # Reads & watches data.json
    ├── Components/
    │   ├── App.razor                  # Root component
    │   ├── Pages/
    │   │   └── Dashboard.razor        # Single page
    │   └── Shared/
    │       ├── MilestoneTimeline.razor # Horizontal timeline strip
    │       ├── StatusCard.razor        # Shipped / In Progress / Carried Over
    │       ├── ProjectHeader.razor     # Project name, dates, health
    │       └── MetricBadge.razor       # KPI indicators
    ├── wwwroot/
    │   ├── css/
    │   │   └── app.css                # Global styles, print styles
    │   └── data/
    │       └── data.json              # Project data file
    └── Properties/
        └── launchSettings.json
```

### Architecture Pattern: **Simple Service + Component Tree**

This project does not need CQRS, MediatR, Clean Architecture, or any layered pattern. The appropriate pattern is:

1. **`DashboardDataService`** — A singleton service registered in DI that reads `data.json` on startup and exposes a `DashboardData` property. Optionally uses `FileSystemWatcher` to reload on file change.
2. **Component Tree** — `Dashboard.razor` injects the service, passes data down to child components via parameters. No state management library needed.
3. **No API layer** — Components read directly from the service. No REST endpoints, no controllers, no middleware.

### Data Model Design

```csharp
public record DashboardData(
    ProjectInfo Project,
    List<Milestone> Milestones,
    List<WorkItem> Shipped,
    List<WorkItem> InProgress,
    List<WorkItem> CarriedOver,
    List<KeyMetric> Metrics
);

public record ProjectInfo(string Name, string Executive, string Status, 
    string ReportDate, string ReportingPeriod);

public record Milestone(string Title, string Date, string Status, string Description);

public record WorkItem(string Title, string Description, string Category, 
    string Priority, string Status);

public record KeyMetric(string Label, string Value, string Trend);
```

### Data Flow

```
data.json  →  DashboardDataService (singleton, file watcher)
                    ↓
           Dashboard.razor (page)
          /        |          \
   Timeline    StatusCards    Metrics
```

### CSS Architecture for Screenshot Fidelity

- Use `@media print` styles for clean screenshot output.
- Fixed-width layout (e.g., 1280px) rather than fluid responsive—ensures consistent screenshots.
- Avoid animations and transitions that could be mid-frame during capture.
- Use explicit `background-color` on all elements (transparent backgrounds screenshot poorly).
- Consider a `?print=true` query parameter that hides browser chrome elements and forces the clean layout.

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only, single-user tool. No auth middleware should be registered. The default Blazor Server template's authentication scaffolding should be removed or never added.

### Data Protection

- `data.json` contains project metadata, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` outside `wwwroot/` and read it from a configurable file path via `appsettings.json`.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local `dotnet run` or `dotnet watch` |
| **Port** | `https://localhost:5001` or `http://localhost:5000` |
| **Distribution** | Self-contained publish: `dotnet publish -c Release -r win-x64 --self-contained` |
| **Executable** | Single-file publish for easy sharing: add `<PublishSingleFile>true</PublishSingleFile>` |
| **No Docker** | Unnecessary for a local single-user tool |
| **No Reverse Proxy** | Kestrel direct is sufficient |
| **No HTTPS in Dev** | Use `http://` to avoid certificate warnings during screenshots |

### Deployment Command

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

This produces a single `.exe` that any team member can run without installing .NET.

### Infrastructure Costs

**$0.** This runs entirely on the developer's local machine.

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Blazor Server overhead for a static page** | Low | Blazor Server's SignalR circuit adds ~50KB overhead but is negligible for local use. Alternatively, consider Blazor Static SSR (new in .NET 8) which renders pure HTML with no circuit. |
| **JSON schema drift** | Medium | Define C# records as the schema contract. Add a startup validation step that logs warnings for missing/extra fields. |
| **Screenshot inconsistency across monitors** | Medium | Use fixed-width layout (1280px). Provide a dedicated `/screenshot` route with `overflow: hidden` and no scrollbar. |
| **FileSystemWatcher reliability on Windows** | Low | Known to occasionally miss events. Implement a fallback polling mechanism (every 5 seconds) as backup. |
| **Over-engineering temptation** | High | This is a screenshot tool, not a SaaS product. Resist adding databases, caching layers, or component libraries unless a specific visual element demands it. |

### Trade-offs Made

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No component library | Must hand-code progress bars, timelines | Full visual control for screenshots; avoids theme conflicts |
| No database | Data lives in a flat file | Single JSON file is trivially editable; no migration headaches |
| Blazor Server over Static SSR | Maintains SignalR circuit unnecessarily | Simpler project template; hot reload works better; trivial overhead locally |
| No API layer | Components coupled to data service | Appropriate for single-page, single-user app; reduces boilerplate by ~60% |
| Custom CSS over Tailwind/Bootstrap | More CSS to write initially | No build pipeline for CSS; no class-name noise in Razor markup |

### Skills Considerations

- Any C# developer can be productive immediately; Blazor `.razor` syntax is intuitive.
- CSS Grid/Flexbox knowledge needed for timeline component—provide a reference implementation.
- `System.Text.Json` deserialization has quirks (e.g., case sensitivity defaults)—use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.

## 7. Open Questions

| # | Question | Impact | Recommended Default |
|---|----------|--------|-------------------|
| 1 | What screen resolution should screenshots target? | Layout dimensions | 1280×900 (standard 16:9 PowerPoint slide at 96 DPI) |
| 2 | Should the timeline be horizontal (top of page) or vertical (sidebar)? | Component layout | Horizontal across top, matching the original HTML design |
| 3 | How many months of data should one report cover? | Data model scope | Single reporting period (1 month) with carry-over from previous |
| 4 | Should the tool support multiple projects or strictly one? | `data.json` schema | Single project per `data.json`; switch projects by swapping files |
| 5 | Is dark mode needed or strictly light theme? | CSS scope | Light theme only (better for PowerPoint embedding) |
| 6 | Should color-coding follow a specific corporate palette? | CSS variables | Use the palette from the original HTML design; expose as CSS custom properties for easy override |
| 7 | Will users edit `data.json` by hand or should there be an edit UI? | Feature scope | Hand-edit for MVP; consider a simple edit form in Phase 2 |

## 8. Implementation Recommendations

### Phase 1: MVP (1-2 days)

**Goal:** Reproduce the original HTML design as a Blazor Server page reading from `data.json`.

1. **Scaffold project** — `dotnet new blazorserver-empty -n ReportingDashboard`
2. **Define data models** — C# records matching the JSON structure
3. **Create `DashboardDataService`** — Read and deserialize `data.json` at startup
4. **Build `Dashboard.razor`** — Single page with all sections inline initially
5. **Port CSS from original HTML** — Translate the existing design's styles into `app.css`
6. **Create sample `data.json`** — Fictional project with realistic milestone data
7. **Verify screenshot quality** — Capture at 1280px width, confirm PowerPoint compatibility

### Phase 2: Polish (1 day)

1. **Extract child components** — `MilestoneTimeline`, `StatusCard`, `MetricBadge`
2. **Add `FileSystemWatcher`** — Auto-refresh when `data.json` is saved
3. **Add print/screenshot CSS** — `@media print` rules, optional `?clean=true` query parameter that hides browser elements
4. **Status color system** — CSS custom properties: `--status-shipped`, `--status-in-progress`, `--status-carried-over`, `--status-at-risk`
5. **Improve timeline** — Add "today" marker, milestone status indicators (complete/in-progress/upcoming)

### Phase 3: Optional Enhancements

1. **Multiple project support** — Dropdown or route parameter to switch `data.json` files
2. **Simple edit form** — Blazor form to modify `data.json` fields without hand-editing
3. **PDF export** — Use browser print-to-PDF (no library needed given the fixed layout)
4. **Historical comparison** — Load previous month's JSON to show delta indicators

### Quick Wins

- **Immediate value:** A working page with the fictional project data demonstrates the concept in under 4 hours.
- **`dotnet watch` workflow:** Edit `data.json` → save → dashboard updates instantly. Demonstrate this loop to stakeholders.
- **Single-file publish:** Distribute as one `.exe` that anyone can double-click to see their project dashboard locally.

### Prototyping Recommendations

- **Prototype the timeline component first** — It's the most visually complex element and the hardest to get right for screenshots. Use pure CSS with `display: flex` and positioned markers before considering any library.
- **Test screenshot quality early** — Take a screenshot on Day 1 and paste it into PowerPoint. Identify font rendering, color, and sizing issues before building out all sections.
- **Validate `data.json` schema with real project data** — Ask a project manager to fill in real milestone data to ensure the schema captures what they actually track.

---

### Appendix: Minimal `data.json` Schema

```json
{
  "project": {
    "name": "Project Phoenix",
    "executive": "Jane Smith, VP Engineering",
    "status": "On Track",
    "reportDate": "2026-04-10",
    "reportingPeriod": "March 2026"
  },
  "milestones": [
    {
      "title": "Architecture Complete",
      "date": "2026-01-15",
      "status": "Completed",
      "description": "System design finalized and approved"
    },
    {
      "title": "Beta Launch",
      "date": "2026-04-01",
      "status": "In Progress",
      "description": "Internal beta with pilot customers"
    }
  ],
  "shipped": [
    {
      "title": "User Authentication Module",
      "description": "OAuth2 + MFA integration complete",
      "category": "Security",
      "priority": "P0"
    }
  ],
  "inProgress": [
    {
      "title": "Dashboard Analytics",
      "description": "Real-time metrics pipeline",
      "category": "Core Feature",
      "priority": "P0",
      "percentComplete": 65
    }
  ],
  "carriedOver": [
    {
      "title": "Mobile Responsive Layout",
      "description": "Deferred from February due to API dependency",
      "category": "UX",
      "priority": "P1",
      "originalTarget": "February 2026",
      "reason": "Blocked by API v2 migration"
    }
  ],
  "metrics": [
    { "label": "Sprint Velocity", "value": "42 pts", "trend": "up" },
    { "label": "Bug Escape Rate", "value": "2.1%", "trend": "down" },
    { "label": "Team Capacity", "value": "85%", "trend": "stable" }
  ]
}
```

### Appendix: Key NuGet Packages (if needed)

| Package | Version | License | Use Case |
|---------|---------|---------|----------|
| `MudBlazor` | 7.x | MIT | Only if timeline/progress components needed |
| `System.Text.Json` | Built-in | MIT | JSON deserialization (no install needed) |
| `Microsoft.Extensions.FileProviders.Physical` | Built-in | MIT | File watching (no install needed) |
| `bUnit` | 1.25.x | MIT | Component testing (dev dependency only) |
| `xUnit` | 2.7.x | Apache-2.0 | Unit testing (dev dependency only) |
