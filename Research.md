# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 09:33 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, and progress in a format optimized for PowerPoint screenshot capture at 1920×1080. The application reads from a local `data.json` file, requires no authentication, no cloud services, and no database — making it an intentionally minimal tool. **Primary recommendation:** Use a bare Blazor Server app with zero JavaScript framework dependencies. Render the timeline SVG inline using Blazor's `MarkupString` or a lightweight SVG builder. Use `System.Text.Json` for config deserialization. Style with scoped CSS matching the existing design's CSS Grid + Flexbox layout. The entire solution should be achievable in a single `.razor` page with one backing model and one service class. ---

### Key Findings

- The original HTML design is fully self-contained (no JS, no external dependencies) — the Blazor implementation should preserve this simplicity.
- The design uses a fixed 1920×1080 viewport, CSS Grid (`160px repeat(4,1fr)`), Flexbox, and inline SVG — all natively supported in Blazor Server without any third-party libraries.
- No charting library is needed; the timeline is a hand-crafted SVG with lines, circles, diamonds, and text — trivially reproducible in Blazor with `@foreach` loops over milestone data.
- The heatmap grid is pure CSS Grid with colored cells — no canvas or charting library required.
- `System.Text.Json` (built into .NET 8) handles all JSON deserialization needs; no Newtonsoft dependency required.
- Blazor Server's SignalR circuit is irrelevant here since this is a read-only, single-user, local-only dashboard — but it does mean the page renders server-side and streams HTML, which is fine.
- For PowerPoint screenshot capture, the fixed-width layout (1920px) should use `min-width: 1920px` on the body to prevent reflow at smaller browser windows.
- No database, no auth, no API layer needed — this is a file-read-and-render application. ---
- Create solution structure with `dotnet new blazorserver-empty`
- Define `DashboardData.cs` POCO models
- Create sample `data.json` with fictional project data
- Implement `DashboardDataService` with `System.Text.Json` deserialization
- Verify data loads correctly with a simple `<pre>@JsonSerializer.Serialize(data)</pre>` dump
- Port the CSS from `OriginalDesignConcept.html` into `Dashboard.razor.css`
- Build the header section (title, subtitle, legend)
- Build the heatmap grid with CSS Grid
- Populate heatmap cells from `data.json` with `@foreach` loops
- Verify visual match against the design screenshot at 1920×1080
- Implement date-to-X-position calculation
- Render milestone swim lanes with `<line>` elements
- Render events as `<circle>` (checkpoints), `<polygon>` (PoC diamonds, production diamonds)
- Add the "NOW" dashed line calculated from current date or config
- Add month labels and grid lines
- Verify alignment against the original design
- Fine-tune colors, spacing, font sizes to pixel-match the design
- Add `overflow: hidden` and fixed body dimensions for clean screenshots
- Test in Edge at 100% zoom on 1920×1080 display
- Create a representative `data.json` for demo/presentation use
- **Start with the CSS.** Copy the `<style>` block from `OriginalDesignConcept.html` verbatim into `Dashboard.razor.css` (with Blazor isolation adjustments). This gives you the visual scaffold immediately.
- **Use `dotnet watch`** from the start. Blazor's hot reload makes CSS iteration extremely fast — change a color, save, see it in the browser instantly.
- **Hardcode first, data-bind second.** Get the HTML structure rendering correctly with hardcoded content, then replace with `@model.Property` bindings. This avoids debugging layout and data issues simultaneously.
- **Screenshot test early.** Take a screenshot after Phase 2 and overlay it with the original design in PowerPoint. This catches layout drift before you invest in the SVG timeline.
```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "workstream": "Trusted Platform · Privacy Automation Workstream",
  "reportMonth": "April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-10",
    "milestones": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "events": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
        ]
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "events": [
          { "date": "2025-12-19", "label": "Dec 19", "type": "checkpoint" },
          { "date": "2026-02-11", "label": "Feb 11", "type": "checkpoint" },
          { "date": "2026-03-15", "label": "Mar 15 PoC", "type": "poc" },
          { "date": "2026-05-15", "label": "May Prod", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "currentColumn": "April",
    "shipped": {
      "January": ["Chatbot v1 intent engine", "MS Role scaffolding"],
      "February": ["PDS schema finalized", "ADO integration"],
      "March": ["PoC demo to stakeholders", "DFD auto-gen prototype"],
      "April": ["Chatbot PoC shipped to staging"]
    },
    "inProgress": {
      "January": ["PDS API design"],
      "February": ["Chatbot NLU training"],
      "March": ["Data Inventory UI", "Review automation rules"],
      "April": ["Production hardening", "Load testing", "Docs & runbook"]
    },
    "carryover": {
      "January": [],
      "February": ["Compliance checklist (from Jan)"],
      "March": ["E2E test suite (from Feb)"],
      "April": ["E2E test suite (from Mar)", "Perf benchmarks"]
    },
    "blockers": {
      "January": [],
      "February": [],
      "March": ["ICM dependency on Platform team"],
      "April": ["Waiting on legal review for PII handling"]
    }
  }
}
``` This project should remain a **~5 file application**:
- `Program.cs` (host setup)
- `DashboardData.cs` (models)
- `DashboardDataService.cs` (JSON reader)
- `Dashboard.razor` (the page)
- `Dashboard.razor.css` (the styles)
- `data.json` (the data) Resist the urge to over-engineer. The original HTML design is 1 file with 0 dependencies. The Blazor version should feel almost that simple, with the added benefit of data-binding from JSON so non-developers can update the content without touching HTML.

### Recommended Tools & Technologies

- | Component | Choice | Version | Notes | |-----------|--------|---------|-------| | Runtime | .NET 8 | 8.0.x (LTS) | Long-term support through Nov 2026 | | UI Framework | Blazor Server | Built into .NET 8 | Server-side rendering, no WASM payload | | Project Template | `blazorserver-empty` | — | Use the empty template to avoid bloat | | Purpose | Library | Version | Notes | |---------|---------|---------|-------| | JSON Deserialization | `System.Text.Json` | Built into .NET 8 | Native, fast, no Newtonsoft needed | | File I/O | `System.IO` | Built into .NET 8 | Read `data.json` from disk | | Dependency Injection | `Microsoft.Extensions.DependencyInjection` | Built into .NET 8 | Register data service as singleton | | Configuration | `IConfiguration` / `appsettings.json` | Built into .NET 8 | Optional: store file path config | | Logging | `Microsoft.Extensions.Logging` | Built into .NET 8 | Console logger sufficient | | Purpose | Approach | Notes | |---------|----------|-------| | CSS Layout | CSS Grid + Flexbox | Matches original design exactly | | CSS Scoping | Blazor CSS Isolation (`Dashboard.razor.css`) | Per-component scoped styles, no leakage | | SVG Timeline | Inline SVG via Razor markup | No charting library; use `@foreach` over milestones | | Fonts | `'Segoe UI', Arial, sans-serif` | System fonts, no web font loading | | Icons/Shapes | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | Diamonds, circles, dashed lines per design | | Color Theming | CSS custom properties (variables) | Define palette once, reference everywhere | | Library | Why Not | |---------|---------| | MudBlazor / Radzen / Syncfusion | Massive component libraries; this is one page with custom layout | | Chart.js / ApexCharts / Plotly | The "chart" is a hand-drawn SVG timeline, not a data chart | | Entity Framework | No database; reading a flat JSON file | | SignalR customization | Default Blazor Server circuit is sufficient | | Newtonsoft.Json | `System.Text.Json` is faster and built-in | | Sass/LESS preprocessors | Vanilla CSS with Blazor isolation is simpler | | Purpose | Library | Version | Notes | |---------|---------|---------|-------| | Unit Tests | xUnit | 2.7.x | Test JSON deserialization, date calculations | | Blazor Component Tests | bUnit | 1.25.x+ | Optional; useful if iterating on layout logic | | Assertions | FluentAssertions | 6.12.x | Readable test assertions | | Purpose | Tool | Notes | |---------|------|-------| | IDE | Visual Studio 2022 or VS Code + C# Dev Kit | Full Blazor support | | Hot Reload | `dotnet watch` | Built into .NET 8, edit Razor and see changes instantly | | Browser DevTools | Edge/Chrome F12 | Inspect CSS Grid layout, verify 1920px rendering | ---
```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── appsettings.json              # Path to data.json
│       ├── Data/
│       │   ├── DashboardData.cs          # POCO models
│       │   └── DashboardDataService.cs   # Reads & deserializes data.json
│       ├── Components/
│       │   ├── App.razor                 # Root component
│       │   ├── Routes.razor
│       │   └── Pages/
│       │       └── Dashboard.razor       # THE single page
│       │       └── Dashboard.razor.css   # Scoped styles
│       └── wwwroot/
│           ├── css/
│           │   └── app.css               # Global resets only
│           └── data.json                 # Dashboard data file
│
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DashboardDataServiceTests.cs
```
```csharp
public record DashboardConfig
{
    public string ProjectName { get; init; }
    public string Workstream { get; init; }
    public string ReportMonth { get; init; }
    public string BacklogUrl { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineEvent> TimelineEvents { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record Milestone
{
    public string Id { get; init; }        // "M1", "M2", "M3"
    public string Label { get; init; }     // "Chatbot & MS Role"
    public string Color { get; init; }     // "#0078D4"
    public List<TimelineEvent> Events { get; init; }
}

public record TimelineEvent
{
    public string Date { get; init; }      // "2026-01-12"
    public string Label { get; init; }     // "Jan 12"
    public string Type { get; init; }      // "checkpoint" | "poc" | "production"
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // ["January", "February", ...]
    public string CurrentColumn { get; init; }  // "April"
    public HeatmapRow Shipped { get; init; }
    public HeatmapRow InProgress { get; init; }
    public HeatmapRow Carryover { get; init; }
    public HeatmapRow Blockers { get; init; }
}

public record HeatmapRow
{
    public Dictionary<string, List<string>> Items { get; init; }
}
```
```
data.json (on disk)
    │
    ▼
DashboardDataService.cs (reads file, deserializes with System.Text.Json)
    │
    ▼
Dashboard.razor (injects service, renders HTML/SVG via Razor syntax)
    │
    ▼
Browser (receives server-rendered HTML over SignalR circuit)
```
- **Single Razor Page:** The entire dashboard is one `Dashboard.razor` component. No routing complexity, no nested layouts beyond the basic `App.razor`.
- **File-based data source:** `DashboardDataService` reads `data.json` from `wwwroot/` or a configurable path. Register as a **Singleton** with `IMemoryCache` or just read on first load and cache in-memory. For a local tool, re-reading on each page load is also fine.
- **Inline SVG for Timeline:** Render SVG directly in Razor markup. Calculate X positions from date ranges:
   ```csharp
   private double GetXPosition(DateOnly date, DateOnly start, DateOnly end, double totalWidth)
       => (date.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber) * totalWidth;
   ```
- **CSS Grid for Heatmap:** Mirror the original design's `grid-template-columns: 160px repeat(4,1fr)` exactly. Apply row-specific background colors via CSS classes (`.ship-cell`, `.prog-cell`, etc.).
- **No Component Library:** The design is bespoke. Using MudBlazor or Radzen would fight the custom layout. Raw HTML + CSS in Blazor gives pixel-perfect control.
- **CSS Custom Properties for Theming:**
   ```css
   :root {
       --color-shipped: #34A853;
       --color-shipped-bg: #F0FBF0;
       --color-progress: #0078D4;
       --color-progress-bg: #EEF4FE;
       --color-carryover: #F4B400;
       --color-carryover-bg: #FFFDE7;
       --color-blocker: #EA4335;
       --color-blocker-bg: #FFF5F5;
       --color-now-line: #EA4335;
   }
   ```
- **Fixed Viewport for Screenshots:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` to ensure consistent rendering for PowerPoint captures. Optionally add a `@media print` block for direct print-to-PDF. ---

### Considerations & Risks

- | Concern | Approach | |---------|----------| | Authentication | **None.** Explicitly out of scope per requirements. | | Authorization | **None.** Single-user local tool. | | Input Validation | Validate `data.json` schema on load; log warnings for missing fields. | | HTTPS | Not required for `localhost`. Default Kestrel HTTP on port 5000 is fine. | | CORS | Not applicable — no cross-origin requests. | | Content Security Policy | Optional but recommended: restrict inline styles if desired. | | Concern | Approach | |---------|----------| | Hosting | **Local Kestrel** — `dotnet run` on developer's machine | | Port | Default `http://localhost:5000` or configure in `launchSettings.json` | | Containerization | Not needed for local-only tool. Optional `Dockerfile` for portability. | | CI/CD | Not needed. Manual `dotnet build && dotnet run`. | | Cloud Services | **None.** Explicitly excluded per requirements. | | Data Storage | Flat `data.json` file on local filesystem | | Monitoring | Console logging via `ILogger<T>`. No APM needed. |
```bash
# One-time
dotnet publish -c Release -o ./publish

# Run
cd publish
dotnet ReportingDashboard.dll
# Open http://localhost:5000
``` Or even simpler during development:
```bash
dotnet watch --project src/ReportingDashboard
``` --- | Risk | Impact | Mitigation | |------|--------|------------| | Blazor Server requires active SignalR connection | Page goes blank if browser disconnects | Acceptable for local-only; refresh the page | | `data.json` schema changes break rendering | Dashboard shows errors or blank sections | Add null-coalescing defaults (`??`) throughout Razor markup | | Fixed 1920×1080 layout doesn't fit smaller screens | Content clipped or scrollable | This is by design for screenshot capture; not a bug | | No hot-reload for `data.json` changes | Must refresh page to see new data | Use `IFileProvider` with `Watch` for auto-reload, or just F5 | | Decision | Trade-off | Justification | |----------|-----------|---------------| | No charting library | Custom SVG code for timeline | The timeline is simple geometry; a charting library would add 500KB+ for no benefit | | No component library | More manual CSS | The design is bespoke; component libraries impose their own design language | | Blazor Server vs. Static HTML | Requires `dotnet` runtime | Enables future extensibility (multiple data sources, filtering) without rewrite | | Single page, no routing | Can't add sub-pages easily | Requirements are explicitly single-page; routing is trivial to add later | | `System.Text.Json` vs. manual parsing | Strict deserialization can fail on malformed JSON | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and try/catch |
- **SVG coordinate math:** The timeline's X-axis mapping from dates to pixel positions is the trickiest part. Off-by-one errors in date math can misalign milestones. **Mitigation:** Write unit tests for `GetXPosition()` with known date/pixel pairs from the original design.
- **CSS Grid browser differences:** The heatmap grid uses specific CSS Grid features. **Mitigation:** Test in the browser you'll use for screenshots (Edge recommended on Windows for Segoe UI font rendering).
- **JSON schema drift:** If `data.json` evolves, nullable fields could cause `NullReferenceException`. **Mitigation:** Make all model properties nullable with `?` suffix and use null-conditional operators in Razor. --- | # | Question | Who Decides | Impact | |---|----------|-------------|--------| | 1 | Should the dashboard auto-refresh when `data.json` changes, or require manual browser refresh? | Product Owner | Determines if `FileSystemWatcher` is needed | | 2 | What date range should the timeline span? Always 6 months? Configurable in `data.json`? | Product Owner | Affects SVG coordinate calculation | | 3 | Should the "Now" line position be calculated from system clock or specified in `data.json`? | Product Owner | `DateTime.Now` is simpler but less controllable for screenshots | | 4 | How many items per heatmap cell before truncation? The original design shows 3-5 per cell. | Designer | May need scrollable cells or tooltip overflow | | 5 | Will there be multiple projects/dashboards, or always one `data.json`? | Product Owner | Determines if we need a project selector or config file switching | | 6 | Is print-to-PDF a requirement alongside screenshots? | Product Owner | Would add `@media print` CSS | | 7 | Should the backlog URL ("ADO Backlog" link) open in browser, or is it decorative for screenshots? | Product Owner | If decorative, can be plain text | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, and progress in a format optimized for PowerPoint screenshot capture at 1920×1080. The application reads from a local `data.json` file, requires no authentication, no cloud services, and no database — making it an intentionally minimal tool.

**Primary recommendation:** Use a bare Blazor Server app with zero JavaScript framework dependencies. Render the timeline SVG inline using Blazor's `MarkupString` or a lightweight SVG builder. Use `System.Text.Json` for config deserialization. Style with scoped CSS matching the existing design's CSS Grid + Flexbox layout. The entire solution should be achievable in a single `.razor` page with one backing model and one service class.

---

## 2. Key Findings

- The original HTML design is fully self-contained (no JS, no external dependencies) — the Blazor implementation should preserve this simplicity.
- The design uses a fixed 1920×1080 viewport, CSS Grid (`160px repeat(4,1fr)`), Flexbox, and inline SVG — all natively supported in Blazor Server without any third-party libraries.
- No charting library is needed; the timeline is a hand-crafted SVG with lines, circles, diamonds, and text — trivially reproducible in Blazor with `@foreach` loops over milestone data.
- The heatmap grid is pure CSS Grid with colored cells — no canvas or charting library required.
- `System.Text.Json` (built into .NET 8) handles all JSON deserialization needs; no Newtonsoft dependency required.
- Blazor Server's SignalR circuit is irrelevant here since this is a read-only, single-user, local-only dashboard — but it does mean the page renders server-side and streams HTML, which is fine.
- For PowerPoint screenshot capture, the fixed-width layout (1920px) should use `min-width: 1920px` on the body to prevent reflow at smaller browser windows.
- No database, no auth, no API layer needed — this is a file-read-and-render application.

---

## 3. Recommended Technology Stack

### Runtime & Framework

| Component | Choice | Version | Notes |
|-----------|--------|---------|-------|
| Runtime | .NET 8 | 8.0.x (LTS) | Long-term support through Nov 2026 |
| UI Framework | Blazor Server | Built into .NET 8 | Server-side rendering, no WASM payload |
| Project Template | `blazorserver-empty` | — | Use the empty template to avoid bloat |

### Core Libraries (All Built-In — Zero External Dependencies)

| Purpose | Library | Version | Notes |
|---------|---------|---------|-------|
| JSON Deserialization | `System.Text.Json` | Built into .NET 8 | Native, fast, no Newtonsoft needed |
| File I/O | `System.IO` | Built into .NET 8 | Read `data.json` from disk |
| Dependency Injection | `Microsoft.Extensions.DependencyInjection` | Built into .NET 8 | Register data service as singleton |
| Configuration | `IConfiguration` / `appsettings.json` | Built into .NET 8 | Optional: store file path config |
| Logging | `Microsoft.Extensions.Logging` | Built into .NET 8 | Console logger sufficient |

### Frontend / Styling

| Purpose | Approach | Notes |
|---------|----------|-------|
| CSS Layout | CSS Grid + Flexbox | Matches original design exactly |
| CSS Scoping | Blazor CSS Isolation (`Dashboard.razor.css`) | Per-component scoped styles, no leakage |
| SVG Timeline | Inline SVG via Razor markup | No charting library; use `@foreach` over milestones |
| Fonts | `'Segoe UI', Arial, sans-serif` | System fonts, no web font loading |
| Icons/Shapes | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | Diamonds, circles, dashed lines per design |
| Color Theming | CSS custom properties (variables) | Define palette once, reference everywhere |

### Explicitly NOT Recommended (Unnecessary Complexity)

| Library | Why Not |
|---------|---------|
| MudBlazor / Radzen / Syncfusion | Massive component libraries; this is one page with custom layout |
| Chart.js / ApexCharts / Plotly | The "chart" is a hand-drawn SVG timeline, not a data chart |
| Entity Framework | No database; reading a flat JSON file |
| SignalR customization | Default Blazor Server circuit is sufficient |
| Newtonsoft.Json | `System.Text.Json` is faster and built-in |
| Sass/LESS preprocessors | Vanilla CSS with Blazor isolation is simpler |

### Testing (Minimal, Optional)

| Purpose | Library | Version | Notes |
|---------|---------|---------|-------|
| Unit Tests | xUnit | 2.7.x | Test JSON deserialization, date calculations |
| Blazor Component Tests | bUnit | 1.25.x+ | Optional; useful if iterating on layout logic |
| Assertions | FluentAssertions | 6.12.x | Readable test assertions |

### Development Tooling

| Purpose | Tool | Notes |
|---------|------|-------|
| IDE | Visual Studio 2022 or VS Code + C# Dev Kit | Full Blazor support |
| Hot Reload | `dotnet watch` | Built into .NET 8, edit Razor and see changes instantly |
| Browser DevTools | Edge/Chrome F12 | Inspect CSS Grid layout, verify 1920px rendering |

---

## 4. Architecture Recommendations

### Solution Structure

```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── appsettings.json              # Path to data.json
│       ├── Data/
│       │   ├── DashboardData.cs          # POCO models
│       │   └── DashboardDataService.cs   # Reads & deserializes data.json
│       ├── Components/
│       │   ├── App.razor                 # Root component
│       │   ├── Routes.razor
│       │   └── Pages/
│       │       └── Dashboard.razor       # THE single page
│       │       └── Dashboard.razor.css   # Scoped styles
│       └── wwwroot/
│           ├── css/
│           │   └── app.css               # Global resets only
│           └── data.json                 # Dashboard data file
│
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DashboardDataServiceTests.cs
```

### Data Model (`DashboardData.cs`)

```csharp
public record DashboardConfig
{
    public string ProjectName { get; init; }
    public string Workstream { get; init; }
    public string ReportMonth { get; init; }
    public string BacklogUrl { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineEvent> TimelineEvents { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record Milestone
{
    public string Id { get; init; }        // "M1", "M2", "M3"
    public string Label { get; init; }     // "Chatbot & MS Role"
    public string Color { get; init; }     // "#0078D4"
    public List<TimelineEvent> Events { get; init; }
}

public record TimelineEvent
{
    public string Date { get; init; }      // "2026-01-12"
    public string Label { get; init; }     // "Jan 12"
    public string Type { get; init; }      // "checkpoint" | "poc" | "production"
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // ["January", "February", ...]
    public string CurrentColumn { get; init; }  // "April"
    public HeatmapRow Shipped { get; init; }
    public HeatmapRow InProgress { get; init; }
    public HeatmapRow Carryover { get; init; }
    public HeatmapRow Blockers { get; init; }
}

public record HeatmapRow
{
    public Dictionary<string, List<string>> Items { get; init; }
}
```

### Data Flow (Trivially Simple)

```
data.json (on disk)
    │
    ▼
DashboardDataService.cs (reads file, deserializes with System.Text.Json)
    │
    ▼
Dashboard.razor (injects service, renders HTML/SVG via Razor syntax)
    │
    ▼
Browser (receives server-rendered HTML over SignalR circuit)
```

### Key Architectural Decisions

1. **Single Razor Page:** The entire dashboard is one `Dashboard.razor` component. No routing complexity, no nested layouts beyond the basic `App.razor`.

2. **File-based data source:** `DashboardDataService` reads `data.json` from `wwwroot/` or a configurable path. Register as a **Singleton** with `IMemoryCache` or just read on first load and cache in-memory. For a local tool, re-reading on each page load is also fine.

3. **Inline SVG for Timeline:** Render SVG directly in Razor markup. Calculate X positions from date ranges:
   ```csharp
   private double GetXPosition(DateOnly date, DateOnly start, DateOnly end, double totalWidth)
       => (date.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber) * totalWidth;
   ```

4. **CSS Grid for Heatmap:** Mirror the original design's `grid-template-columns: 160px repeat(4,1fr)` exactly. Apply row-specific background colors via CSS classes (`.ship-cell`, `.prog-cell`, etc.).

5. **No Component Library:** The design is bespoke. Using MudBlazor or Radzen would fight the custom layout. Raw HTML + CSS in Blazor gives pixel-perfect control.

6. **CSS Custom Properties for Theming:**
   ```css
   :root {
       --color-shipped: #34A853;
       --color-shipped-bg: #F0FBF0;
       --color-progress: #0078D4;
       --color-progress-bg: #EEF4FE;
       --color-carryover: #F4B400;
       --color-carryover-bg: #FFFDE7;
       --color-blocker: #EA4335;
       --color-blocker-bg: #FFF5F5;
       --color-now-line: #EA4335;
   }
   ```

7. **Fixed Viewport for Screenshots:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` to ensure consistent rendering for PowerPoint captures. Optionally add a `@media print` block for direct print-to-PDF.

---

## 5. Security & Infrastructure

### Security (Intentionally Minimal)

| Concern | Approach |
|---------|----------|
| Authentication | **None.** Explicitly out of scope per requirements. |
| Authorization | **None.** Single-user local tool. |
| Input Validation | Validate `data.json` schema on load; log warnings for missing fields. |
| HTTPS | Not required for `localhost`. Default Kestrel HTTP on port 5000 is fine. |
| CORS | Not applicable — no cross-origin requests. |
| Content Security Policy | Optional but recommended: restrict inline styles if desired. |

### Infrastructure & Deployment

| Concern | Approach |
|---------|----------|
| Hosting | **Local Kestrel** — `dotnet run` on developer's machine |
| Port | Default `http://localhost:5000` or configure in `launchSettings.json` |
| Containerization | Not needed for local-only tool. Optional `Dockerfile` for portability. |
| CI/CD | Not needed. Manual `dotnet build && dotnet run`. |
| Cloud Services | **None.** Explicitly excluded per requirements. |
| Data Storage | Flat `data.json` file on local filesystem |
| Monitoring | Console logging via `ILogger<T>`. No APM needed. |

### Deployment Steps (For End Users)

```bash
# One-time
dotnet publish -c Release -o ./publish

# Run
cd publish
dotnet ReportingDashboard.dll
# Open http://localhost:5000
```

Or even simpler during development:
```bash
dotnet watch --project src/ReportingDashboard
```

---

## 6. Risks & Trade-offs

### Low-Risk Items (Acceptable)

| Risk | Impact | Mitigation |
|------|--------|------------|
| Blazor Server requires active SignalR connection | Page goes blank if browser disconnects | Acceptable for local-only; refresh the page |
| `data.json` schema changes break rendering | Dashboard shows errors or blank sections | Add null-coalescing defaults (`??`) throughout Razor markup |
| Fixed 1920×1080 layout doesn't fit smaller screens | Content clipped or scrollable | This is by design for screenshot capture; not a bug |
| No hot-reload for `data.json` changes | Must refresh page to see new data | Use `IFileProvider` with `Watch` for auto-reload, or just F5 |

### Trade-offs Made

| Decision | Trade-off | Justification |
|----------|-----------|---------------|
| No charting library | Custom SVG code for timeline | The timeline is simple geometry; a charting library would add 500KB+ for no benefit |
| No component library | More manual CSS | The design is bespoke; component libraries impose their own design language |
| Blazor Server vs. Static HTML | Requires `dotnet` runtime | Enables future extensibility (multiple data sources, filtering) without rewrite |
| Single page, no routing | Can't add sub-pages easily | Requirements are explicitly single-page; routing is trivial to add later |
| `System.Text.Json` vs. manual parsing | Strict deserialization can fail on malformed JSON | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and try/catch |

### Potential Pitfalls

1. **SVG coordinate math:** The timeline's X-axis mapping from dates to pixel positions is the trickiest part. Off-by-one errors in date math can misalign milestones. **Mitigation:** Write unit tests for `GetXPosition()` with known date/pixel pairs from the original design.

2. **CSS Grid browser differences:** The heatmap grid uses specific CSS Grid features. **Mitigation:** Test in the browser you'll use for screenshots (Edge recommended on Windows for Segoe UI font rendering).

3. **JSON schema drift:** If `data.json` evolves, nullable fields could cause `NullReferenceException`. **Mitigation:** Make all model properties nullable with `?` suffix and use null-conditional operators in Razor.

---

## 7. Open Questions

| # | Question | Who Decides | Impact |
|---|----------|-------------|--------|
| 1 | Should the dashboard auto-refresh when `data.json` changes, or require manual browser refresh? | Product Owner | Determines if `FileSystemWatcher` is needed |
| 2 | What date range should the timeline span? Always 6 months? Configurable in `data.json`? | Product Owner | Affects SVG coordinate calculation |
| 3 | Should the "Now" line position be calculated from system clock or specified in `data.json`? | Product Owner | `DateTime.Now` is simpler but less controllable for screenshots |
| 4 | How many items per heatmap cell before truncation? The original design shows 3-5 per cell. | Designer | May need scrollable cells or tooltip overflow |
| 5 | Will there be multiple projects/dashboards, or always one `data.json`? | Product Owner | Determines if we need a project selector or config file switching |
| 6 | Is print-to-PDF a requirement alongside screenshots? | Product Owner | Would add `@media print` CSS |
| 7 | Should the backlog URL ("ADO Backlog" link) open in browser, or is it decorative for screenshots? | Product Owner | If decorative, can be plain text |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Skeleton + Data Model (1-2 hours)
- Create solution structure with `dotnet new blazorserver-empty`
- Define `DashboardData.cs` POCO models
- Create sample `data.json` with fictional project data
- Implement `DashboardDataService` with `System.Text.Json` deserialization
- Verify data loads correctly with a simple `<pre>@JsonSerializer.Serialize(data)</pre>` dump

#### Phase 2: Layout + Heatmap (2-3 hours)
- Port the CSS from `OriginalDesignConcept.html` into `Dashboard.razor.css`
- Build the header section (title, subtitle, legend)
- Build the heatmap grid with CSS Grid
- Populate heatmap cells from `data.json` with `@foreach` loops
- Verify visual match against the design screenshot at 1920×1080

#### Phase 3: SVG Timeline (2-3 hours)
- Implement date-to-X-position calculation
- Render milestone swim lanes with `<line>` elements
- Render events as `<circle>` (checkpoints), `<polygon>` (PoC diamonds, production diamonds)
- Add the "NOW" dashed line calculated from current date or config
- Add month labels and grid lines
- Verify alignment against the original design

#### Phase 4: Polish + Screenshot Optimization (1 hour)
- Fine-tune colors, spacing, font sizes to pixel-match the design
- Add `overflow: hidden` and fixed body dimensions for clean screenshots
- Test in Edge at 100% zoom on 1920×1080 display
- Create a representative `data.json` for demo/presentation use

### Quick Wins

1. **Start with the CSS.** Copy the `<style>` block from `OriginalDesignConcept.html` verbatim into `Dashboard.razor.css` (with Blazor isolation adjustments). This gives you the visual scaffold immediately.

2. **Use `dotnet watch`** from the start. Blazor's hot reload makes CSS iteration extremely fast — change a color, save, see it in the browser instantly.

3. **Hardcode first, data-bind second.** Get the HTML structure rendering correctly with hardcoded content, then replace with `@model.Property` bindings. This avoids debugging layout and data issues simultaneously.

4. **Screenshot test early.** Take a screenshot after Phase 2 and overlay it with the original design in PowerPoint. This catches layout drift before you invest in the SVG timeline.

### Example `data.json` Structure

```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "workstream": "Trusted Platform · Privacy Automation Workstream",
  "reportMonth": "April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-10",
    "milestones": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "events": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
        ]
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "events": [
          { "date": "2025-12-19", "label": "Dec 19", "type": "checkpoint" },
          { "date": "2026-02-11", "label": "Feb 11", "type": "checkpoint" },
          { "date": "2026-03-15", "label": "Mar 15 PoC", "type": "poc" },
          { "date": "2026-05-15", "label": "May Prod", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "currentColumn": "April",
    "shipped": {
      "January": ["Chatbot v1 intent engine", "MS Role scaffolding"],
      "February": ["PDS schema finalized", "ADO integration"],
      "March": ["PoC demo to stakeholders", "DFD auto-gen prototype"],
      "April": ["Chatbot PoC shipped to staging"]
    },
    "inProgress": {
      "January": ["PDS API design"],
      "February": ["Chatbot NLU training"],
      "March": ["Data Inventory UI", "Review automation rules"],
      "April": ["Production hardening", "Load testing", "Docs & runbook"]
    },
    "carryover": {
      "January": [],
      "February": ["Compliance checklist (from Jan)"],
      "March": ["E2E test suite (from Feb)"],
      "April": ["E2E test suite (from Mar)", "Perf benchmarks"]
    },
    "blockers": {
      "January": [],
      "February": [],
      "March": ["ICM dependency on Platform team"],
      "April": ["Waiting on legal review for PII handling"]
    }
  }
}
```

### Final Note on Simplicity

This project should remain a **~5 file application**:
- `Program.cs` (host setup)
- `DashboardData.cs` (models)
- `DashboardDataService.cs` (JSON reader)
- `Dashboard.razor` (the page)
- `Dashboard.razor.css` (the styles)
- `data.json` (the data)

Resist the urge to over-engineer. The original HTML design is 1 file with 0 dependencies. The Blazor version should feel almost that simple, with the added benefit of data-binding from JSON so non-developers can update the content without touching HTML.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/5b4f6777f59bd7734ef69e70bf6517178470f41f/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
