# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 04:04 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint decks to executives. **Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for data loading. No database, no authentication, no external services. The entire solution should be under 10 files and deployable with `dotnet run`. ---

### Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** layouts that translate directly to Blazor component markup with zero JavaScript required.
- **Inline SVG** is used for the timeline/Gantt visualization (milestones, date markers, "NOW" line). Blazor Server renders SVG natively in Razor markup — no charting library is needed.
- The design targets a fixed **1920×1080 resolution** for screenshot capture, eliminating the need for responsive design or mobile considerations.
- A `data.json` flat file is the ideal storage mechanism — no database is warranted for this scope. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's **SignalR connection** is irrelevant for this use case (single user, local only) but comes free with the hosting model and causes no harm.
- The color palette, typography (Segoe UI), and layout dimensions are fully specified in the HTML reference and should be replicated exactly via scoped CSS in the Blazor component.
- **No third-party NuGet packages are required.** The entire project can be built with the .NET 8 SDK alone.
- The heatmap grid has four status rows (Shipped/green, In Progress/blue, Carryover/amber, Blockers/red) × N month columns — this maps cleanly to a `@foreach` loop over data-driven columns and rows.
- The SVG timeline milestones (diamonds for PoC/Production, circles for checkpoints, dashed "NOW" line) can be computed from JSON date ranges and rendered as parameterized SVG elements.
- Screenshot fidelity is the #1 quality metric — this means exact color hex values, precise spacing, and consistent font rendering matter more than interactivity or performance. ---
- Create the .sln and Blazor Server project scaffold
- Port the HTML/CSS from `OriginalDesignConcept.html` directly into `Dashboard.razor` as static markup
- Verify pixel-perfect match at 1920×1080 in browser
- **Deliverable:** A running page that looks identical to the design but with hardcoded content
- Define `DashboardData.cs` model classes matching the JSON schema
- Create `data.json` with fictional project data
- Build `DashboardDataService.cs` to load and deserialize the file
- Replace hardcoded markup with `@foreach` loops and `@bind` expressions
- Extract `Timeline.razor`, `Heatmap.razor`, `HeatmapCell.razor` components
- **Deliverable:** Fully data-driven dashboard; change `data.json` → restart → see updated dashboard
- Add `FileSystemWatcher` for live `data.json` reload without restart
- Add CSS custom properties for easy theme tweaking
- Add a "Last Updated" timestamp in the header
- Parameterize the SVG viewBox dimensions for different track counts
- Write basic xUnit + bUnit tests for data loading and component rendering
- **Deliverable:** Production-ready (for its limited scope) dashboard
- **Copy-paste the CSS** from `OriginalDesignConcept.html` into `app.css` — this gets you 80% of the visual design in minutes.
- **Use `dotnet new blazor --interactivity Server`** (.NET 8 template) to scaffold the project in one command.
- **Use `dotnet watch`** during development for hot-reload — edit Razor files and see changes instantly.
- **Set the browser to 1920×1080** using DevTools device toolbar for consistent screenshot dimensions.
- **SVG date-to-pixel mapping**: Build the `DateToX()` function and test with edge cases (milestones at month boundaries, overlapping labels) before wiring up the full timeline. Off-by-one errors in coordinate math are common.
- **CSS Grid dynamic columns**: Test that `grid-template-columns: 160px repeat(N, 1fr)` renders correctly when N varies from 3 to 6 months. Ensure cell content doesn't overflow.
- **JSON schema**: Write the `data.json` for the fictional project first, then build the C# model to match — not the other way around. The JSON should be human-friendly since it's the primary "admin interface." --- For the example `data.json`, use a fictional project like **"Project Atlas — Cloud Migration Platform"** with:
- **3 milestone tracks**: M1 (API Gateway), M2 (Data Migration Engine), M3 (Monitoring Dashboard)
- **4 months**: January–April 2026
- **Shipped items**: 6–8 items across months showing steady delivery
- **In Progress items**: 3–4 items concentrated in the current month
- **Carryover items**: 1–2 items that slipped from prior months
- **Blockers**: 1 item to show the red status row is functional This gives a realistic density that will look credible in executive presentations without being so full that cells overflow.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` — no extra package needed | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the original HTML design exactly; no CSS framework needed | | **SVG Timeline** | Inline SVG in Razor | N/A | Native browser SVG rendering; parameterized via C# code | | **Styling** | Scoped CSS (`.razor.css`) | Built-in | Component-isolated styles prevent bleed; maps 1:1 to the design's CSS | | **Font** | Segoe UI (system font) | N/A | Available on all Windows machines; no web font loading needed | **Why no charting library?** Libraries like `Radzen.Blazor` (v5.x), `MudBlazor` (v7.x), or `ApexCharts.Blazor` add unnecessary complexity. The timeline is a simple SVG with positioned elements (lines, circles, diamonds, text). Hand-crafted SVG in Razor gives pixel-perfect control needed for executive screenshots without the overhead of a charting abstraction. **Why no CSS framework (Bootstrap, Tailwind)?** The design has exactly one page with a fixed layout. Adding Bootstrap's 150KB+ or Tailwind's build pipeline for a few flex/grid containers is unjustified. The original HTML achieves the design in ~80 lines of CSS. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Runtime** | .NET 8.0 LTS | 8.0.x | Long-term support until November 2026 | | **Web Host** | Kestrel (built-in) | 8.0.x | `dotnet run` serves the app locally on `https://localhost:5001` | | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Source-generated serialization available; `JsonSerializer.Deserialize<T>()` is sufficient | | **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload dashboard when `data.json` is edited | | Layer | Technology | Notes | |-------|-----------|-------| | **Primary Store** | `data.json` flat file | Single JSON file in the project root or `wwwroot/data/` directory | | **Format** | JSON | Human-editable, version-controllable, zero infrastructure | | **No Database** | N/A | SQLite, LiteDB, etc. are unnecessary for a single config file | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Testing** | xUnit | 2.7.x | .NET ecosystem standard; `dotnet new xunit` scaffold | | **Blazor Component Testing** | bUnit | 1.28.x+ | Renders Blazor components in-memory for assertions | | **Assertions** | FluentAssertions | 6.12.x | Optional but improves test readability | | Layer | Technology | Notes | |-------|-----------|-------| | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Full Blazor hot-reload support | | **Build** | `dotnet build` | No additional build tooling needed | | **Run** | `dotnet run` or `dotnet watch` | Hot-reload during development | | **Screenshot Capture** | Browser DevTools or Snipping Tool | Manual; no automation needed for occasional PowerPoint use | --- This project does not warrant Clean Architecture, CQRS, or microservices. The recommended pattern is intentionally simple:
```
ReportingDashboard.sln
├── ReportingDashboard/
│   ├── Program.cs                    # Minimal host builder
│   ├── Components/
│   │   ├── App.razor                 # Root component
│   │   ├── Pages/
│   │   │   └── Dashboard.razor       # The single page (route: "/")
│   │   ├── Layout/
│   │   │   └── MainLayout.razor      # Minimal layout wrapper
│   │   └── Shared/
│   │       ├── Timeline.razor        # SVG timeline component
│   │       ├── Heatmap.razor         # CSS Grid heatmap component
│   │       └── HeatmapCell.razor     # Individual cell with items
│   ├── Models/
│   │   └── DashboardData.cs          # C# POCOs matching data.json shape
│   ├── Services/
│   │   └── DashboardDataService.cs   # Loads & caches data.json
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css               # Global styles matching the design
│   │   └── data/
│   │       └── data.json             # The data source
│   └── ReportingDashboard.csproj
```
```
data.json → DashboardDataService (reads & deserializes on startup / on-demand)
          → Dashboard.razor (injects service, passes data to child components)
          → Timeline.razor (renders SVG from milestone data)
          → Heatmap.razor (renders CSS Grid from status/month/item data)
```
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
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
    "months": ["January", "February", "March", "April"],
    "currentMonth": "April",
    "rows": [
      {
        "status": "shipped",
        "items": {
          "January": ["Chatbot v1 launched", "MS Role tool live"],
          "February": ["PDS integration"],
          "March": ["Auto Review DFD PoC"],
          "April": ["Data Inventory GA"]
        }
      }
    ]
  }
}
``` | Component | Responsibility | Rendering Strategy | |-----------|---------------|-------------------| | `Dashboard.razor` | Page layout, data injection, header with legend | Flexbox column layout matching `.hdr` + `.tl-area` + `.hm-wrap` | | `Timeline.razor` | SVG timeline with tracks, milestones, NOW line | Inline `<svg>` with C#-computed `x` positions based on date math | | `Heatmap.razor` | CSS Grid with header row, status rows, data cells | `grid-template-columns: 160px repeat(N, 1fr)` where N = month count | | `HeatmapCell.razor` | Individual cell rendering bullet-pointed items | Iterates items list, applies status-specific CSS class for dot color | The timeline maps dates to x-coordinates within the SVG viewBox:
```csharp
private double DateToX(DateTime date)
{
    var totalDays = (EndDate - StartDate).TotalDays;
    var elapsed = (date - StartDate).TotalDays;
    return (elapsed / totalDays) * SvgWidth;
}
``` Milestone types map to SVG shapes:
- **Checkpoint** → `<circle>` (small gray dot)
- **PoC** → `<polygon>` (gold diamond, rotated square)
- **Production** → `<polygon>` (green diamond, rotated square)
- **NOW line** → `<line>` (red dashed vertical line with `stroke-dasharray="5,3"`) Replicate the original HTML's CSS directly into `app.css` or scoped `.razor.css` files:
- **Fixed 1920×1080 viewport**: Set `body { width: 1920px; height: 1080px; overflow: hidden; }` — this ensures consistent screenshot output.
- **Color tokens**: Define CSS custom properties for the palette:
  ```css
  :root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
  }
  ```
- **Grid layout**: Exactly matches the original `grid-template-columns: 160px repeat(4,1fr)` but dynamically driven by month count. ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The application runs locally (`localhost`) for a single user generating screenshots. Adding authentication would be over-engineering. If future needs arise, the simplest addition would be:
- `Microsoft.AspNetCore.Authentication.Cookies` with a hardcoded admin user — but this is not recommended for the current scope.
- **No sensitive data**: The dashboard displays project status information, not PII or credentials.
- **`data.json` is local only**: No network transmission of data. The file sits on the developer's machine.
- **No encryption needed**: The data is not sensitive enough to warrant encryption at rest. | Aspect | Recommendation | |--------|---------------| | **Host** | Local Kestrel via `dotnet run` | | **URL** | `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | `dotnet publish -c Release` → run the executable directly | | **Distribution** | Zip the publish output; any Windows machine with .NET 8 runtime can run it | | **Self-contained option** | `dotnet publish -c Release --self-contained -r win-x64` for machines without .NET runtime | **$0.** This runs entirely on the developer's local machine. No servers, no cloud services, no databases, no ongoing costs. --- | Risk | Severity | Mitigation | |------|----------|------------| | **SVG rendering inconsistencies across browsers** | Low | Target a single browser (Edge/Chrome) for screenshots; test SVG output once and lock it | | **`data.json` schema drift** | Low | Define a C# POCO model; deserialization will throw on structural mismatches, catching errors early | | **Blazor Server SignalR overhead for a static page** | Very Low | Negligible for single-user local use; if ever a concern, switch to Blazor Static SSR (available in .NET 8) | | **Font rendering differences across machines** | Low | Segoe UI is standard on Windows; if sharing with Mac users, add a fallback font stack | | **Over-engineering temptation** | Medium | Resist adding databases, auth, caching layers, or DI abstractions. The project's value is simplicity | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | No charting library | Must hand-craft SVG | Full pixel control; design has exactly one SVG timeline — a library adds more complexity than it removes | | No CSS framework | Must write all CSS manually | ~100 lines of CSS total; frameworks add 100KB+ for no benefit | | No database | Must edit JSON by hand | The "admin interface" is a text editor; this is acceptable for a single-user tool | | No responsive design | Only works at 1920×1080 | Explicitly designed for screenshot capture at this resolution | | Blazor Server over Blazor WASM | Requires .NET runtime on host | Faster startup, simpler debugging, SSR renders the page immediately — ideal for screenshot workflows | None meaningful. This is a single-page, single-user, read-only dashboard with a ~1KB data file. --- | # | Question | Stakeholder | Impact | |---|----------|------------|--------| | 1 | **How many months should the heatmap display?** The original design shows 4 (Jan–Apr). Should this be configurable in `data.json` or always show a rolling window? | Product Owner | Affects grid column calculation and JSON schema | | 2 | **How many timeline tracks?** The original shows 3 (M1, M2, M3). Is there a maximum? | Product Owner | Affects SVG height calculation | | 3 | **Should the "NOW" line auto-calculate from system date or be set in `data.json`?** | Developer preference | Auto-calculation is simpler but `data.json` control allows screenshot reproducibility | | 4 | **Is the ADO Backlog link functional or just placeholder text?** | Product Owner | If functional, it's a simple `<a href>` from `data.json` | | 5 | **Will this ever need to run on macOS/Linux?** | Team | Affects self-contained publish target and font fallback strategy | | 6 | **Should `data.json` support multiple projects, or is it always one project per instance?** | Product Owner | Affects whether we need a project selector or just a single flat file | | 7 | **Is hot-reload of `data.json` needed (edit file → page updates)?** | Developer preference | Easy to add with `FileSystemWatcher` + Blazor `StateHasChanged()` but adds slight complexity | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint decks to executives.

**Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for data loading. No database, no authentication, no external services. The entire solution should be under 10 files and deployable with `dotnet run`.

---

## 2. Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** layouts that translate directly to Blazor component markup with zero JavaScript required.
- **Inline SVG** is used for the timeline/Gantt visualization (milestones, date markers, "NOW" line). Blazor Server renders SVG natively in Razor markup — no charting library is needed.
- The design targets a fixed **1920×1080 resolution** for screenshot capture, eliminating the need for responsive design or mobile considerations.
- A `data.json` flat file is the ideal storage mechanism — no database is warranted for this scope. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's **SignalR connection** is irrelevant for this use case (single user, local only) but comes free with the hosting model and causes no harm.
- The color palette, typography (Segoe UI), and layout dimensions are fully specified in the HTML reference and should be replicated exactly via scoped CSS in the Blazor component.
- **No third-party NuGet packages are required.** The entire project can be built with the .NET 8 SDK alone.
- The heatmap grid has four status rows (Shipped/green, In Progress/blue, Carryover/amber, Blockers/red) × N month columns — this maps cleanly to a `@foreach` loop over data-driven columns and rows.
- The SVG timeline milestones (diamonds for PoC/Production, circles for checkpoints, dashed "NOW" line) can be computed from JSON date ranges and rendered as parameterized SVG elements.
- Screenshot fidelity is the #1 quality metric — this means exact color hex values, precise spacing, and consistent font rendering matter more than interactivity or performance.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` — no extra package needed |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the original HTML design exactly; no CSS framework needed |
| **SVG Timeline** | Inline SVG in Razor | N/A | Native browser SVG rendering; parameterized via C# code |
| **Styling** | Scoped CSS (`.razor.css`) | Built-in | Component-isolated styles prevent bleed; maps 1:1 to the design's CSS |
| **Font** | Segoe UI (system font) | N/A | Available on all Windows machines; no web font loading needed |

**Why no charting library?** Libraries like `Radzen.Blazor` (v5.x), `MudBlazor` (v7.x), or `ApexCharts.Blazor` add unnecessary complexity. The timeline is a simple SVG with positioned elements (lines, circles, diamonds, text). Hand-crafted SVG in Razor gives pixel-perfect control needed for executive screenshots without the overhead of a charting abstraction.

**Why no CSS framework (Bootstrap, Tailwind)?** The design has exactly one page with a fixed layout. Adding Bootstrap's 150KB+ or Tailwind's build pipeline for a few flex/grid containers is unjustified. The original HTML achieves the design in ~80 lines of CSS.

### Backend

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Runtime** | .NET 8.0 LTS | 8.0.x | Long-term support until November 2026 |
| **Web Host** | Kestrel (built-in) | 8.0.x | `dotnet run` serves the app locally on `https://localhost:5001` |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Source-generated serialization available; `JsonSerializer.Deserialize<T>()` is sufficient |
| **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload dashboard when `data.json` is edited |

### Data Storage

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Primary Store** | `data.json` flat file | Single JSON file in the project root or `wwwroot/data/` directory |
| **Format** | JSON | Human-editable, version-controllable, zero infrastructure |
| **No Database** | N/A | SQLite, LiteDB, etc. are unnecessary for a single config file |

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Testing** | xUnit | 2.7.x | .NET ecosystem standard; `dotnet new xunit` scaffold |
| **Blazor Component Testing** | bUnit | 1.28.x+ | Renders Blazor components in-memory for assertions |
| **Assertions** | FluentAssertions | 6.12.x | Optional but improves test readability |

### Infrastructure & Tooling

| Layer | Technology | Notes |
|-------|-----------|-------|
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Full Blazor hot-reload support |
| **Build** | `dotnet build` | No additional build tooling needed |
| **Run** | `dotnet run` or `dotnet watch` | Hot-reload during development |
| **Screenshot Capture** | Browser DevTools or Snipping Tool | Manual; no automation needed for occasional PowerPoint use |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Component Page with Service-Backed Data**

This project does not warrant Clean Architecture, CQRS, or microservices. The recommended pattern is intentionally simple:

```
ReportingDashboard.sln
├── ReportingDashboard/
│   ├── Program.cs                    # Minimal host builder
│   ├── Components/
│   │   ├── App.razor                 # Root component
│   │   ├── Pages/
│   │   │   └── Dashboard.razor       # The single page (route: "/")
│   │   ├── Layout/
│   │   │   └── MainLayout.razor      # Minimal layout wrapper
│   │   └── Shared/
│   │       ├── Timeline.razor        # SVG timeline component
│   │       ├── Heatmap.razor         # CSS Grid heatmap component
│   │       └── HeatmapCell.razor     # Individual cell with items
│   ├── Models/
│   │   └── DashboardData.cs          # C# POCOs matching data.json shape
│   ├── Services/
│   │   └── DashboardDataService.cs   # Loads & caches data.json
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css               # Global styles matching the design
│   │   └── data/
│   │       └── data.json             # The data source
│   └── ReportingDashboard.csproj
```

### Data Flow

```
data.json → DashboardDataService (reads & deserializes on startup / on-demand)
          → Dashboard.razor (injects service, passes data to child components)
          → Timeline.razor (renders SVG from milestone data)
          → Heatmap.razor (renders CSS Grid from status/month/item data)
```

### Data Model Design (`data.json` shape)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
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
    "months": ["January", "February", "March", "April"],
    "currentMonth": "April",
    "rows": [
      {
        "status": "shipped",
        "items": {
          "January": ["Chatbot v1 launched", "MS Role tool live"],
          "February": ["PDS integration"],
          "March": ["Auto Review DFD PoC"],
          "April": ["Data Inventory GA"]
        }
      }
    ]
  }
}
```

### Component Responsibilities

| Component | Responsibility | Rendering Strategy |
|-----------|---------------|-------------------|
| `Dashboard.razor` | Page layout, data injection, header with legend | Flexbox column layout matching `.hdr` + `.tl-area` + `.hm-wrap` |
| `Timeline.razor` | SVG timeline with tracks, milestones, NOW line | Inline `<svg>` with C#-computed `x` positions based on date math |
| `Heatmap.razor` | CSS Grid with header row, status rows, data cells | `grid-template-columns: 160px repeat(N, 1fr)` where N = month count |
| `HeatmapCell.razor` | Individual cell rendering bullet-pointed items | Iterates items list, applies status-specific CSS class for dot color |

### SVG Timeline Coordinate Calculation

The timeline maps dates to x-coordinates within the SVG viewBox:

```csharp
private double DateToX(DateTime date)
{
    var totalDays = (EndDate - StartDate).TotalDays;
    var elapsed = (date - StartDate).TotalDays;
    return (elapsed / totalDays) * SvgWidth;
}
```

Milestone types map to SVG shapes:
- **Checkpoint** → `<circle>` (small gray dot)
- **PoC** → `<polygon>` (gold diamond, rotated square)
- **Production** → `<polygon>` (green diamond, rotated square)
- **NOW line** → `<line>` (red dashed vertical line with `stroke-dasharray="5,3"`)

### CSS Strategy

Replicate the original HTML's CSS directly into `app.css` or scoped `.razor.css` files:

- **Fixed 1920×1080 viewport**: Set `body { width: 1920px; height: 1080px; overflow: hidden; }` — this ensures consistent screenshot output.
- **Color tokens**: Define CSS custom properties for the palette:
  ```css
  :root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
  }
  ```
- **Grid layout**: Exactly matches the original `grid-template-columns: 160px repeat(4,1fr)` but dynamically driven by month count.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The application runs locally (`localhost`) for a single user generating screenshots. Adding authentication would be over-engineering.

If future needs arise, the simplest addition would be:
- `Microsoft.AspNetCore.Authentication.Cookies` with a hardcoded admin user — but this is not recommended for the current scope.

### Data Protection

- **No sensitive data**: The dashboard displays project status information, not PII or credentials.
- **`data.json` is local only**: No network transmission of data. The file sits on the developer's machine.
- **No encryption needed**: The data is not sensitive enough to warrant encryption at rest.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Host** | Local Kestrel via `dotnet run` |
| **URL** | `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | `dotnet publish -c Release` → run the executable directly |
| **Distribution** | Zip the publish output; any Windows machine with .NET 8 runtime can run it |
| **Self-contained option** | `dotnet publish -c Release --self-contained -r win-x64` for machines without .NET runtime |

### Infrastructure Costs

**$0.** This runs entirely on the developer's local machine. No servers, no cloud services, no databases, no ongoing costs.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **SVG rendering inconsistencies across browsers** | Low | Target a single browser (Edge/Chrome) for screenshots; test SVG output once and lock it |
| **`data.json` schema drift** | Low | Define a C# POCO model; deserialization will throw on structural mismatches, catching errors early |
| **Blazor Server SignalR overhead for a static page** | Very Low | Negligible for single-user local use; if ever a concern, switch to Blazor Static SSR (available in .NET 8) |
| **Font rendering differences across machines** | Low | Segoe UI is standard on Windows; if sharing with Mac users, add a fallback font stack |
| **Over-engineering temptation** | Medium | Resist adding databases, auth, caching layers, or DI abstractions. The project's value is simplicity |

### Trade-offs Made

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No charting library | Must hand-craft SVG | Full pixel control; design has exactly one SVG timeline — a library adds more complexity than it removes |
| No CSS framework | Must write all CSS manually | ~100 lines of CSS total; frameworks add 100KB+ for no benefit |
| No database | Must edit JSON by hand | The "admin interface" is a text editor; this is acceptable for a single-user tool |
| No responsive design | Only works at 1920×1080 | Explicitly designed for screenshot capture at this resolution |
| Blazor Server over Blazor WASM | Requires .NET runtime on host | Faster startup, simpler debugging, SSR renders the page immediately — ideal for screenshot workflows |

### Bottlenecks

None meaningful. This is a single-page, single-user, read-only dashboard with a ~1KB data file.

---

## 7. Open Questions

| # | Question | Stakeholder | Impact |
|---|----------|------------|--------|
| 1 | **How many months should the heatmap display?** The original design shows 4 (Jan–Apr). Should this be configurable in `data.json` or always show a rolling window? | Product Owner | Affects grid column calculation and JSON schema |
| 2 | **How many timeline tracks?** The original shows 3 (M1, M2, M3). Is there a maximum? | Product Owner | Affects SVG height calculation |
| 3 | **Should the "NOW" line auto-calculate from system date or be set in `data.json`?** | Developer preference | Auto-calculation is simpler but `data.json` control allows screenshot reproducibility |
| 4 | **Is the ADO Backlog link functional or just placeholder text?** | Product Owner | If functional, it's a simple `<a href>` from `data.json` |
| 5 | **Will this ever need to run on macOS/Linux?** | Team | Affects self-contained publish target and font fallback strategy |
| 6 | **Should `data.json` support multiple projects, or is it always one project per instance?** | Product Owner | Affects whether we need a project selector or just a single flat file |
| 7 | **Is hot-reload of `data.json` needed (edit file → page updates)?** | Developer preference | Easy to add with `FileSystemWatcher` + Blazor `StateHasChanged()` but adds slight complexity |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Static Replica (Day 1) — **MVP**
- Create the .sln and Blazor Server project scaffold
- Port the HTML/CSS from `OriginalDesignConcept.html` directly into `Dashboard.razor` as static markup
- Verify pixel-perfect match at 1920×1080 in browser
- **Deliverable:** A running page that looks identical to the design but with hardcoded content

#### Phase 2: Data-Driven Rendering (Day 1–2)
- Define `DashboardData.cs` model classes matching the JSON schema
- Create `data.json` with fictional project data
- Build `DashboardDataService.cs` to load and deserialize the file
- Replace hardcoded markup with `@foreach` loops and `@bind` expressions
- Extract `Timeline.razor`, `Heatmap.razor`, `HeatmapCell.razor` components
- **Deliverable:** Fully data-driven dashboard; change `data.json` → restart → see updated dashboard

#### Phase 3: Polish & Quality of Life (Day 2–3)
- Add `FileSystemWatcher` for live `data.json` reload without restart
- Add CSS custom properties for easy theme tweaking
- Add a "Last Updated" timestamp in the header
- Parameterize the SVG viewBox dimensions for different track counts
- Write basic xUnit + bUnit tests for data loading and component rendering
- **Deliverable:** Production-ready (for its limited scope) dashboard

### Quick Wins

1. **Copy-paste the CSS** from `OriginalDesignConcept.html` into `app.css` — this gets you 80% of the visual design in minutes.
2. **Use `dotnet new blazor --interactivity Server`** (.NET 8 template) to scaffold the project in one command.
3. **Use `dotnet watch`** during development for hot-reload — edit Razor files and see changes instantly.
4. **Set the browser to 1920×1080** using DevTools device toolbar for consistent screenshot dimensions.

### Areas Where Prototyping Is Recommended

1. **SVG date-to-pixel mapping**: Build the `DateToX()` function and test with edge cases (milestones at month boundaries, overlapping labels) before wiring up the full timeline. Off-by-one errors in coordinate math are common.
2. **CSS Grid dynamic columns**: Test that `grid-template-columns: 160px repeat(N, 1fr)` renders correctly when N varies from 3 to 6 months. Ensure cell content doesn't overflow.
3. **JSON schema**: Write the `data.json` for the fictional project first, then build the C# model to match — not the other way around. The JSON should be human-friendly since it's the primary "admin interface."

---

## Appendix: Fictional Project Data Suggestion

For the example `data.json`, use a fictional project like **"Project Atlas — Cloud Migration Platform"** with:

- **3 milestone tracks**: M1 (API Gateway), M2 (Data Migration Engine), M3 (Monitoring Dashboard)
- **4 months**: January–April 2026
- **Shipped items**: 6–8 items across months showing steady delivery
- **In Progress items**: 3–4 items concentrated in the current month
- **Carryover items**: 1–2 items that slipped from prior months
- **Blockers**: 1 item to show the red status row is functional

This gives a realistic density that will look credible in executive presentations without being so full that cells overflow.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/3911c6425c693f00916a51746fa9c46eb9c37013/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
