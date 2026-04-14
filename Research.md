# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-14 00:15 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard reads project data from a `data.json` file and renders a pixel-perfect view of milestones, timelines, and status heatmaps designed for screenshot capture into PowerPoint decks. **Primary Recommendation:** Use a minimal Blazor Server application with zero external JavaScript framework dependencies. Leverage inline SVG rendering via Blazor components for the timeline/Gantt chart, CSS Grid + Flexbox for the heatmap layout, and `System.Text.Json` for config deserialization. No database is needed—`data.json` is the sole data source. The entire solution should be a single `.sln` with one Blazor Server project, deployable via `dotnet run` with no setup ceremony. The design reference (OriginalDesignConcept.html) is already a self-contained HTML/CSS/SVG page. The Blazor implementation should decompose it into ~5 Razor components while preserving the exact CSS and SVG structure. This is intentionally simple—no auth, no database, no enterprise patterns. ---

### Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) for the heatmap and **Flexbox** for header/layout—both are natively supported in Blazor Server with standard CSS files.
- The timeline section uses **inline SVG** with `<line>`, `<circle>`, `<polygon>`, `<text>`, and `<feDropShadow>` filter elements. Blazor can render SVG directly in `.razor` files with full data-binding support—no charting library is needed.
- The design targets a fixed **1920×1080** resolution for screenshot capture, which simplifies responsive design concerns (none needed).
- A `data.json` configuration file is sufficient for all data needs: milestones, status items, dates, and category assignments. No database, no API, no ORM.
- The color palette is fully defined in the HTML reference: green (#34A853) for shipped, blue (#0078D4) for in-progress, amber (#F4B400) for carryover, red (#EA4335) for blockers. These map directly to CSS classes.
- **Blazor Server in .NET 8** supports static SSR (Server-Side Rendering) mode, which is ideal here—renders the page once, minimal interactivity needed.
- The font is **Segoe UI**, which is pre-installed on Windows (the target environment). No web font loading required.
- Total estimated development effort: **1-2 days** for a senior developer familiar with Blazor. --- **Goal:** Render the dashboard with hardcoded fake data, matching the design pixel-for-pixel.
- `dotnet new blazor --interactivity Server -n ReportingDashboard`
- Create `data.json` with fictional project data (e.g., "Project Phoenix" with milestones like "Alpha Release", "Beta Launch", "GA")
- Create C# record models: `DashboardData`, `Milestone`, `MilestoneEvent`, `StatusCategory`
- Build `DashboardDataService` — reads and deserializes `data.json`
- Port CSS from `OriginalDesignConcept.html` into `dashboard.css`
- Build components: `Header.razor`, `TimelineSection.razor`, `HeatmapGrid.razor`, `HeatmapCell.razor`
- Verify visual match against design screenshot at 1920×1080 **Deliverable:** `dotnet run` → open browser → visually identical to design reference.
- Make heatmap columns dynamic (driven by `months` array in JSON)
- Make milestone swim lanes dynamic (driven by `milestones` array)
- Auto-calculate "Now" marker position from `DateTime.Now`
- Calculate SVG X coordinates from date math (map date range to pixel range)
- Add `grid-template-columns` dynamic generation based on month count
- Add CSS `@media print` styles for clean print-to-PDF if needed
- Test with 3-6 months of data to verify layout flexibility **Deliverable:** Edit `data.json` → refresh browser → new data renders correctly.
- **Copy-paste the CSS** from the HTML reference directly. It's already well-structured and uses standard properties. Don't rewrite it.
- **Use C# 12 `required` keyword** on record properties for built-in validation of `data.json` structure.
- **Use `IFileSystemWatcher`** (optional, Phase 3) to auto-reload `data.json` on change without browser refresh.
- **Add `<meta name="viewport">` removal** — the fixed 1920×1080 design should NOT have a responsive viewport meta tag.
- ❌ User authentication or login page
- ❌ Admin UI for editing data (just edit the JSON file)
- ❌ Database or Entity Framework
- ❌ REST API endpoints
- ❌ Docker containerization
- ❌ CI/CD pipeline
- ❌ Logging framework (Console.WriteLine is fine)
- ❌ Multiple pages or routing
- ❌ Dark mode or theme switching
- ❌ Mobile responsive layout Place `data.json` in the project root (next to `.csproj`) and configure it as a content file:
```xml
<ItemGroup>
  <Content Include="data.json" CopyToOutputDirectory="Always" />
</ItemGroup>
``` This ensures it's available in the publish output and editable without recompilation.
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── data.json
    ├── Models/
    │   └── DashboardData.cs          # Records: DashboardData, Milestone, etc.
    ├── Services/
    │   └── DashboardDataService.cs   # Reads & deserializes data.json
    ├── Components/
    │   ├── App.razor
    │   ├── Pages/
    │   │   └── Dashboard.razor       # Main page
    │   └── Layout/
    │       ├── Header.razor
    │       ├── TimelineSection.razor
    │       ├── HeatmapGrid.razor
    │       └── HeatmapCell.razor
    └── wwwroot/
        └── css/
            └── dashboard.css         # Ported from OriginalDesignConcept.html
``` --- | Package | Version | Required? | Purpose | |---------|---------|-----------|---------| | `Microsoft.AspNetCore.Components` | 8.0.x | ✅ Included | Blazor Server runtime | | `System.Text.Json` | 8.0.x | ✅ Included | JSON deserialization | | `bUnit` | 1.28+ | Optional | Component unit testing | | `xUnit` | 2.7+ | Optional | Test runner | **Total external NuGet packages required: 0.** Everything needed ships with the .NET 8 SDK.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **UI Framework** | Blazor Server (.NET 8) | .NET 8.0.x (LTS) | Component rendering, data binding | | **CSS Layout** | Native CSS Grid + Flexbox | CSS3 | Heatmap grid, header layout | | **Charts/Timeline** | Inline SVG via Razor | N/A | Milestone diamonds, timeline lines, circles | | **Styling** | Scoped CSS (`.razor.css`) | Built-in | Per-component styles matching design | | **Icons/Shapes** | Raw SVG markup | N/A | Diamond milestones, circle checkpoints | **No external CSS frameworks** (Bootstrap, Tailwind, etc.)—the design is precise and self-contained. Adding a framework would fight the design rather than help it. **No charting libraries** (Chart.js, Radzen Charts, etc.)—the timeline SVG is simple enough to render directly. A charting library would add complexity and reduce pixel-level control over the exact design. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` | | **File I/O** | `System.IO` | Built into .NET 8 | Read `data.json` from disk | | **DI / Config** | `IConfiguration` or direct file read | Built-in | Load data at startup | | **Data Models** | C# record types | C# 12 | Strongly-typed data models | **No database.** The `data.json` file is read on startup (or on each page load for live editing). This is intentional—executives update a JSON file or it's generated by a separate process. | Component | Technology | Notes | |-----------|-----------|-------| | **Solution** | `.sln` file | Single solution | | **Project** | Blazor Server (`.csproj`) | Single project, `dotnet new blazor --interactivity Server` | | **Hosting** | Kestrel (built-in) | `dotnet run`, opens on `https://localhost:5001` | | Tool | Version | Purpose | |------|---------|---------| | **bUnit** | 1.28+ | Blazor component unit tests (optional) | | **xUnit** | 2.7+ | Test runner | Testing is optional given the simplicity but recommended for the data model parsing logic. --- This is a single-page dashboard. Do **not** use:
- ❌ Clean Architecture / Onion Architecture
- ❌ CQRS / MediatR
- ❌ Repository pattern
- ❌ Microservices
- ❌ State management libraries (Fluxor, etc.)
- ✅ A single Blazor Server project
- ✅ A `Models/` folder with C# records for the JSON schema
- ✅ A `Services/DashboardDataService.cs` that reads and deserializes `data.json`
- ✅ A `Components/` folder with 4-5 Razor components Map directly to the HTML design's visual sections:
```
Components/
├── DashboardPage.razor          # Main page, orchestrates layout
├── Header.razor                 # Title, subtitle, legend icons
├── TimelineSection.razor        # SVG milestone Gantt chart
├── HeatmapGrid.razor            # CSS Grid status matrix
└── HeatmapCell.razor            # Individual cell with bullet items
```
```
data.json (disk)
    ↓ read at startup or per-request
DashboardDataService.cs
    ↓ deserialized into C# records
DashboardPage.razor
    ↓ passes data via [Parameter] or CascadingValue
Header / Timeline / Heatmap components
    ↓ render HTML/SVG/CSS
Browser (screenshot target)
```
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-14",
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
      ]
    }
  ],
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": [],
        "Apr": ["Item D", "Item E"]
      }
    }
  ]
}
``` Port the CSS from `OriginalDesignConcept.html` directly into either:
- **A single `DashboardPage.razor.css` scoped stylesheet** (simplest), or
- **A global `wwwroot/css/dashboard.css`** file The design CSS is already production-quality. Preserve it as-is, with these Blazor adaptations:
- Use `::deep` combinator if using scoped CSS with child components
- Keep the `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot-perfect rendering
- The `grid-template-columns: 160px repeat(4,1fr)` must be data-driven (column count = month count) The SVG timeline should be rendered directly in Razor with data binding:
```razor
<svg width="@svgWidth" height="185" style="overflow:visible">
  @foreach (var milestone in Data.Milestones)
  {
    <line x1="0" y1="@milestone.Y" x2="@svgWidth" y2="@milestone.Y"
          stroke="@milestone.Color" stroke-width="3"/>
    @foreach (var evt in milestone.Events)
    {
      @if (evt.Type == "poc")
      {
        <polygon points="@DiamondPoints(evt.X, milestone.Y)"
                 fill="#F4B400" filter="url(#sh)"/>
      }
    }
  }
</svg>
``` This gives full control over positioning and matches the design exactly—something no charting library could guarantee. ---

### Considerations & Risks

- Per requirements, **no authentication or authorization**. This is a local-only dashboard.
- No HTTPS certificate needed for local use (Kestrel dev cert is fine)
- No CORS configuration
- No API keys or secrets
- `data.json` is a local file—no injection concerns **One consideration:** If the dashboard is ever shared on a network, add `builder.WebHost.UseUrls("http://localhost:5000")` to bind only to localhost. | Concern | Approach | |---------|----------| | **Runtime** | `dotnet run` from command line | | **Port** | Default Kestrel (localhost:5000/5001) | | **Process** | Manual start, no Windows Service needed | | **Updates** | Edit `data.json`, refresh browser | | **Screenshot workflow** | Open in browser → F11 fullscreen → screenshot tool |
- **`dotnet run`** — Developer runs from source. Simplest.
- **`dotnet publish -c Release --self-contained`** — Single-folder deployment, no .NET SDK needed on target machine. Include `data.json` in publish output.
- **Single-file publish** — `dotnet publish -c Release --self-contained -p:PublishSingleFile=true` — One `.exe` + `data.json`. **Recommended:** Option 2 or 3 for sharing with non-developers. **$0.** This runs locally. No cloud, no hosting, no licenses. .NET 8 is MIT-licensed. Visual Studio Code or VS Community Edition are free. --- | Risk | Impact | Mitigation | |------|--------|------------| | **JSON schema drift** | Dashboard crashes on bad data | Add JSON schema validation or defensive null checks in C# models; use `required` properties on records | | **SVG positioning math** | Timeline items misaligned | Calculate X positions from date ranges using a helper method; unit test the math | | **Browser rendering differences** | Screenshots look different across browsers | Standardize on **Edge/Chrome** (both Chromium-based, consistent rendering). Document the browser choice. | | **Scoped CSS `::deep` issues** | Styles don't apply to child components | Use a global CSS file if scoped CSS causes friction | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | No database | Can't query historical data | Simplicity; JSON file is sufficient for current-month reporting | | No auth | Anyone on the machine can view | Local-only use case; not network-exposed | | Fixed 1920×1080 | Not responsive on mobile/tablets | Designed for desktop screenshot capture only | | No charting library | Must hand-code SVG positioning | Full pixel control; design is simple enough; avoids dependency bloat | | Blazor Server (not WASM) | Requires .NET runtime | Simpler deployment; no WASM download; instant render | | No real-time updates | Must refresh browser after editing JSON | Acceptable for monthly reporting cadence |
- Scalability — Single user, local machine
- High availability — Not a production service
- Data privacy — No sensitive data in the dashboard itself
- Performance — Single page, <50 DOM elements in heatmap, renders in <100ms --- | # | Question | Who Decides | Default if No Answer | |---|----------|-------------|---------------------| | 1 | How many months should the heatmap display? (Design shows 4) | Product Owner | 4 months (current + 3 prior) | | 2 | Should `data.json` be hand-edited or generated from ADO/external source? | Project Lead | Hand-edited initially; generation can be added later | | 3 | Should the page auto-refresh when `data.json` changes? | Developer | No — manual browser refresh is fine | | 4 | Is the "ADO Backlog" link in the header functional or decorative? | Product Owner | Functional hyperlink to ADO URL from `data.json` | | 5 | How many milestone swim lanes (M1, M2, M3…) should be supported? | Product Owner | 3 (matching design), but data-driven so any number works | | 6 | Should the "Now" marker on the timeline auto-calculate from system date? | Developer | Yes — use `DateTime.Now` to position the red dashed line | | 7 | Will the color palette ever change per-project? | Product Owner | No — hardcode the 4-category color scheme from the design | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard reads project data from a `data.json` file and renders a pixel-perfect view of milestones, timelines, and status heatmaps designed for screenshot capture into PowerPoint decks.

**Primary Recommendation:** Use a minimal Blazor Server application with zero external JavaScript framework dependencies. Leverage inline SVG rendering via Blazor components for the timeline/Gantt chart, CSS Grid + Flexbox for the heatmap layout, and `System.Text.Json` for config deserialization. No database is needed—`data.json` is the sole data source. The entire solution should be a single `.sln` with one Blazor Server project, deployable via `dotnet run` with no setup ceremony.

The design reference (OriginalDesignConcept.html) is already a self-contained HTML/CSS/SVG page. The Blazor implementation should decompose it into ~5 Razor components while preserving the exact CSS and SVG structure. This is intentionally simple—no auth, no database, no enterprise patterns.

---

## 2. Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) for the heatmap and **Flexbox** for header/layout—both are natively supported in Blazor Server with standard CSS files.
- The timeline section uses **inline SVG** with `<line>`, `<circle>`, `<polygon>`, `<text>`, and `<feDropShadow>` filter elements. Blazor can render SVG directly in `.razor` files with full data-binding support—no charting library is needed.
- The design targets a fixed **1920×1080** resolution for screenshot capture, which simplifies responsive design concerns (none needed).
- A `data.json` configuration file is sufficient for all data needs: milestones, status items, dates, and category assignments. No database, no API, no ORM.
- The color palette is fully defined in the HTML reference: green (#34A853) for shipped, blue (#0078D4) for in-progress, amber (#F4B400) for carryover, red (#EA4335) for blockers. These map directly to CSS classes.
- **Blazor Server in .NET 8** supports static SSR (Server-Side Rendering) mode, which is ideal here—renders the page once, minimal interactivity needed.
- The font is **Segoe UI**, which is pre-installed on Windows (the target environment). No web font loading required.
- Total estimated development effort: **1-2 days** for a senior developer familiar with Blazor.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **UI Framework** | Blazor Server (.NET 8) | .NET 8.0.x (LTS) | Component rendering, data binding |
| **CSS Layout** | Native CSS Grid + Flexbox | CSS3 | Heatmap grid, header layout |
| **Charts/Timeline** | Inline SVG via Razor | N/A | Milestone diamonds, timeline lines, circles |
| **Styling** | Scoped CSS (`.razor.css`) | Built-in | Per-component styles matching design |
| **Icons/Shapes** | Raw SVG markup | N/A | Diamond milestones, circle checkpoints |

**No external CSS frameworks** (Bootstrap, Tailwind, etc.)—the design is precise and self-contained. Adding a framework would fight the design rather than help it.

**No charting libraries** (Chart.js, Radzen Charts, etc.)—the timeline SVG is simple enough to render directly. A charting library would add complexity and reduce pixel-level control over the exact design.

### Backend / Data Layer

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` |
| **File I/O** | `System.IO` | Built into .NET 8 | Read `data.json` from disk |
| **DI / Config** | `IConfiguration` or direct file read | Built-in | Load data at startup |
| **Data Models** | C# record types | C# 12 | Strongly-typed data models |

**No database.** The `data.json` file is read on startup (or on each page load for live editing). This is intentional—executives update a JSON file or it's generated by a separate process.

### Project Structure

| Component | Technology | Notes |
|-----------|-----------|-------|
| **Solution** | `.sln` file | Single solution |
| **Project** | Blazor Server (`.csproj`) | Single project, `dotnet new blazor --interactivity Server` |
| **Hosting** | Kestrel (built-in) | `dotnet run`, opens on `https://localhost:5001` |

### Testing (Lightweight)

| Tool | Version | Purpose |
|------|---------|---------|
| **bUnit** | 1.28+ | Blazor component unit tests (optional) |
| **xUnit** | 2.7+ | Test runner |

Testing is optional given the simplicity but recommended for the data model parsing logic.

---

## 4. Architecture Recommendations

### Pattern: Flat Component Architecture (No Over-Engineering)

This is a single-page dashboard. Do **not** use:
- ❌ Clean Architecture / Onion Architecture
- ❌ CQRS / MediatR
- ❌ Repository pattern
- ❌ Microservices
- ❌ State management libraries (Fluxor, etc.)

**Do use:**
- ✅ A single Blazor Server project
- ✅ A `Models/` folder with C# records for the JSON schema
- ✅ A `Services/DashboardDataService.cs` that reads and deserializes `data.json`
- ✅ A `Components/` folder with 4-5 Razor components

### Recommended Component Decomposition

Map directly to the HTML design's visual sections:

```
Components/
├── DashboardPage.razor          # Main page, orchestrates layout
├── Header.razor                 # Title, subtitle, legend icons
├── TimelineSection.razor        # SVG milestone Gantt chart
├── HeatmapGrid.razor            # CSS Grid status matrix
└── HeatmapCell.razor            # Individual cell with bullet items
```

### Data Flow

```
data.json (disk)
    ↓ read at startup or per-request
DashboardDataService.cs
    ↓ deserialized into C# records
DashboardPage.razor
    ↓ passes data via [Parameter] or CascadingValue
Header / Timeline / Heatmap components
    ↓ render HTML/SVG/CSS
Browser (screenshot target)
```

### Data Model Design (`data.json` schema)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-14",
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
      ]
    }
  ],
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": [],
        "Apr": ["Item D", "Item E"]
      }
    }
  ]
}
```

### CSS Strategy

Port the CSS from `OriginalDesignConcept.html` directly into either:
1. **A single `DashboardPage.razor.css` scoped stylesheet** (simplest), or
2. **A global `wwwroot/css/dashboard.css`** file

The design CSS is already production-quality. Preserve it as-is, with these Blazor adaptations:
- Use `::deep` combinator if using scoped CSS with child components
- Keep the `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot-perfect rendering
- The `grid-template-columns: 160px repeat(4,1fr)` must be data-driven (column count = month count)

### SVG Timeline Rendering

The SVG timeline should be rendered directly in Razor with data binding:

```razor
<svg width="@svgWidth" height="185" style="overflow:visible">
  @foreach (var milestone in Data.Milestones)
  {
    <line x1="0" y1="@milestone.Y" x2="@svgWidth" y2="@milestone.Y"
          stroke="@milestone.Color" stroke-width="3"/>
    @foreach (var evt in milestone.Events)
    {
      @if (evt.Type == "poc")
      {
        <polygon points="@DiamondPoints(evt.X, milestone.Y)"
                 fill="#F4B400" filter="url(#sh)"/>
      }
    }
  }
</svg>
```

This gives full control over positioning and matches the design exactly—something no charting library could guarantee.

---

## 5. Security & Infrastructure

### Security: Intentionally Minimal

Per requirements, **no authentication or authorization**. This is a local-only dashboard.

- No HTTPS certificate needed for local use (Kestrel dev cert is fine)
- No CORS configuration
- No API keys or secrets
- `data.json` is a local file—no injection concerns

**One consideration:** If the dashboard is ever shared on a network, add `builder.WebHost.UseUrls("http://localhost:5000")` to bind only to localhost.

### Hosting & Deployment

| Concern | Approach |
|---------|----------|
| **Runtime** | `dotnet run` from command line |
| **Port** | Default Kestrel (localhost:5000/5001) |
| **Process** | Manual start, no Windows Service needed |
| **Updates** | Edit `data.json`, refresh browser |
| **Screenshot workflow** | Open in browser → F11 fullscreen → screenshot tool |

### Deployment Options (Simplest to Most Formal)

1. **`dotnet run`** — Developer runs from source. Simplest.
2. **`dotnet publish -c Release --self-contained`** — Single-folder deployment, no .NET SDK needed on target machine. Include `data.json` in publish output.
3. **Single-file publish** — `dotnet publish -c Release --self-contained -p:PublishSingleFile=true` — One `.exe` + `data.json`.

**Recommended:** Option 2 or 3 for sharing with non-developers.

### Infrastructure Costs

**$0.** This runs locally. No cloud, no hosting, no licenses. .NET 8 is MIT-licensed. Visual Studio Code or VS Community Edition are free.

---

## 6. Risks & Trade-offs

### Low Risks (Manageable)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **JSON schema drift** | Dashboard crashes on bad data | Add JSON schema validation or defensive null checks in C# models; use `required` properties on records |
| **SVG positioning math** | Timeline items misaligned | Calculate X positions from date ranges using a helper method; unit test the math |
| **Browser rendering differences** | Screenshots look different across browsers | Standardize on **Edge/Chrome** (both Chromium-based, consistent rendering). Document the browser choice. |
| **Scoped CSS `::deep` issues** | Styles don't apply to child components | Use a global CSS file if scoped CSS causes friction |

### Trade-offs Made (Intentional)

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No database | Can't query historical data | Simplicity; JSON file is sufficient for current-month reporting |
| No auth | Anyone on the machine can view | Local-only use case; not network-exposed |
| Fixed 1920×1080 | Not responsive on mobile/tablets | Designed for desktop screenshot capture only |
| No charting library | Must hand-code SVG positioning | Full pixel control; design is simple enough; avoids dependency bloat |
| Blazor Server (not WASM) | Requires .NET runtime | Simpler deployment; no WASM download; instant render |
| No real-time updates | Must refresh browser after editing JSON | Acceptable for monthly reporting cadence |

### Non-Risks (Things That Don't Apply)

- Scalability — Single user, local machine
- High availability — Not a production service
- Data privacy — No sensitive data in the dashboard itself
- Performance — Single page, <50 DOM elements in heatmap, renders in <100ms

---

## 7. Open Questions

| # | Question | Who Decides | Default if No Answer |
|---|----------|-------------|---------------------|
| 1 | How many months should the heatmap display? (Design shows 4) | Product Owner | 4 months (current + 3 prior) |
| 2 | Should `data.json` be hand-edited or generated from ADO/external source? | Project Lead | Hand-edited initially; generation can be added later |
| 3 | Should the page auto-refresh when `data.json` changes? | Developer | No — manual browser refresh is fine |
| 4 | Is the "ADO Backlog" link in the header functional or decorative? | Product Owner | Functional hyperlink to ADO URL from `data.json` |
| 5 | How many milestone swim lanes (M1, M2, M3…) should be supported? | Product Owner | 3 (matching design), but data-driven so any number works |
| 6 | Should the "Now" marker on the timeline auto-calculate from system date? | Developer | Yes — use `DateTime.Now` to position the red dashed line |
| 7 | Will the color palette ever change per-project? | Product Owner | No — hardcode the 4-category color scheme from the design |

---

## 8. Implementation Recommendations

### Phase 1: MVP (Day 1) — Static Render

**Goal:** Render the dashboard with hardcoded fake data, matching the design pixel-for-pixel.

1. `dotnet new blazor --interactivity Server -n ReportingDashboard`
2. Create `data.json` with fictional project data (e.g., "Project Phoenix" with milestones like "Alpha Release", "Beta Launch", "GA")
3. Create C# record models: `DashboardData`, `Milestone`, `MilestoneEvent`, `StatusCategory`
4. Build `DashboardDataService` — reads and deserializes `data.json`
5. Port CSS from `OriginalDesignConcept.html` into `dashboard.css`
6. Build components: `Header.razor`, `TimelineSection.razor`, `HeatmapGrid.razor`, `HeatmapCell.razor`
7. Verify visual match against design screenshot at 1920×1080

**Deliverable:** `dotnet run` → open browser → visually identical to design reference.

### Phase 2: Data-Driven Polish (Day 2)

1. Make heatmap columns dynamic (driven by `months` array in JSON)
2. Make milestone swim lanes dynamic (driven by `milestones` array)
3. Auto-calculate "Now" marker position from `DateTime.Now`
4. Calculate SVG X coordinates from date math (map date range to pixel range)
5. Add `grid-template-columns` dynamic generation based on month count
6. Add CSS `@media print` styles for clean print-to-PDF if needed
7. Test with 3-6 months of data to verify layout flexibility

**Deliverable:** Edit `data.json` → refresh browser → new data renders correctly.

### Quick Wins

- **Copy-paste the CSS** from the HTML reference directly. It's already well-structured and uses standard properties. Don't rewrite it.
- **Use C# 12 `required` keyword** on record properties for built-in validation of `data.json` structure.
- **Use `IFileSystemWatcher`** (optional, Phase 3) to auto-reload `data.json` on change without browser refresh.
- **Add `<meta name="viewport">` removal** — the fixed 1920×1080 design should NOT have a responsive viewport meta tag.

### What NOT to Build

- ❌ User authentication or login page
- ❌ Admin UI for editing data (just edit the JSON file)
- ❌ Database or Entity Framework
- ❌ REST API endpoints
- ❌ Docker containerization
- ❌ CI/CD pipeline
- ❌ Logging framework (Console.WriteLine is fine)
- ❌ Multiple pages or routing
- ❌ Dark mode or theme switching
- ❌ Mobile responsive layout

### Recommended `data.json` Location

Place `data.json` in the project root (next to `.csproj`) and configure it as a content file:

```xml
<ItemGroup>
  <Content Include="data.json" CopyToOutputDirectory="Always" />
</ItemGroup>
```

This ensures it's available in the publish output and editable without recompilation.

### Solution Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── data.json
    ├── Models/
    │   └── DashboardData.cs          # Records: DashboardData, Milestone, etc.
    ├── Services/
    │   └── DashboardDataService.cs   # Reads & deserializes data.json
    ├── Components/
    │   ├── App.razor
    │   ├── Pages/
    │   │   └── Dashboard.razor       # Main page
    │   └── Layout/
    │       ├── Header.razor
    │       ├── TimelineSection.razor
    │       ├── HeatmapGrid.razor
    │       └── HeatmapCell.razor
    └── wwwroot/
        └── css/
            └── dashboard.css         # Ported from OriginalDesignConcept.html
```

---

## Appendix: Key NuGet Packages

| Package | Version | Required? | Purpose |
|---------|---------|-----------|---------|
| `Microsoft.AspNetCore.Components` | 8.0.x | ✅ Included | Blazor Server runtime |
| `System.Text.Json` | 8.0.x | ✅ Included | JSON deserialization |
| `bUnit` | 1.28+ | Optional | Component unit testing |
| `xUnit` | 2.7+ | Optional | Test runner |

**Total external NuGet packages required: 0.** Everything needed ships with the .NET 8 SDK.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/35fc419fbcee15e62c013dafec38c2a646c45143/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
