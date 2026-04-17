# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 06:27 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint executive decks. **Primary Recommendation:** Build a minimal Blazor Server app with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for deserializing `data.json`. No database, no authentication, no external services. The entire app should be ~5-8 files total. Prioritize visual fidelity to the HTML design reference over architectural sophistication — this is a utility tool, not an enterprise platform. ---

### Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid (`160px repeat(4,1fr)`), Flexbox, inline SVG for the timeline, and the Segoe UI font family. Blazor Server can reproduce this exactly using identical CSS.
- **No database is needed.** A flat `data.json` file is the ideal storage for this use case — it's human-editable, version-controllable, and trivially deserialized with `System.Text.Json`.
- **No charting library is needed.** The timeline is a hand-crafted SVG with lines, circles, diamonds, and text labels. Blazor's Razor syntax can generate SVG elements directly from data bindings — no third-party library required.
- The heatmap grid is pure CSS Grid with colored cells. No canvas, no WebGL, no charting framework. Blazor components map 1:1 to the HTML structure.
- Blazor Server's SignalR connection is irrelevant for this use case (single user, local, read-only), but it works fine and adds no complexity. Blazor Server is simpler to set up than Blazor WebAssembly for a local tool.
- The fixed 1920×1080 viewport is ideal for screenshot capture. No responsive design is needed — in fact, responsive design should be **avoided** to ensure consistent screenshots.
- The entire project can be implemented in a single afternoon. The architecture should reflect this simplicity. ---
- Create the .NET 8 Blazor Server solution (`dotnet new blazor --name ReportingDashboard --interactivity Server`)
- Port the CSS from `OriginalDesignConcept.html` into `dashboard.css`
- Create the `Dashboard.razor` page with hardcoded HTML matching the reference
- Verify pixel-perfect rendering at 1920×1080 **Milestone:** Static HTML dashboard renders identically to the reference design.
- Define C# POCO models: `DashboardData`, `Milestone`, `MilestoneEvent`, `StatusCategory`, `MonthItems`
- Create `data.json` with fictional project data
- Build `DashboardDataService` to deserialize `data.json`
- Replace hardcoded HTML with Razor data bindings (`@foreach`, `@if`, string interpolation) **Milestone:** Dashboard renders from `data.json`. Changing the JSON changes the display.
- Build `TimelineSection.razor` component
- Implement date-to-pixel mapping utility
- Render milestone swimlanes, markers (circles, diamonds), month gridlines, "NOW" line
- Test with different date ranges to ensure layout doesn't break **Milestone:** Timeline accurately plots milestones from JSON data.
- Fine-tune spacing, font sizes, colors to match reference
- Add CSS custom properties for easy re-theming
- Test screenshot workflow (browser → screenshot → PowerPoint)
- Add a `README.md` explaining how to edit `data.json` and take screenshots **Milestone:** Production-ready screenshot utility.
- **CSS custom properties** for color theming — swap project branding in seconds
- **Static SSR mode** — eliminates the SignalR connection, faster page load, simpler
- **`dotnet watch`** during development — auto-refreshes when `data.json` or Razor files change
- **Browser DevTools device mode** — force exact 1920×1080 viewport without resizing the window
- ❌ Authentication or user management
- ❌ Database or ORM layer
- ❌ REST API endpoints
- ❌ Real-time updates or SignalR push
- ❌ Responsive/mobile layout
- ❌ Admin panel for editing data
- ❌ PDF export (use screenshots instead)
- ❌ Caching layer (data is read once, it's tiny)
- ❌ Logging framework (console output is fine)
- ❌ Docker containerization
- ❌ CI/CD pipeline **5-8 hours** for a developer familiar with Blazor, or **1-2 days** for someone learning Blazor basics. The HTML reference already solves the hardest problem (visual design), so the implementation is primarily a porting exercise with data-binding added.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | Server-side rendering with Razor components | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Reproduces the exact grid from the HTML reference | | **Timeline Visualization** | Inline SVG via Razor | N/A | Milestone diamonds, checkpoint circles, month gridlines, "NOW" marker | | **Font** | Segoe UI (system font) | N/A | Matches design spec; no web font loading needed on Windows | | **Icons/Shapes** | Pure CSS + SVG | N/A | Diamond markers via `transform:rotate(45deg)`, circles via `border-radius` | **No additional frontend NuGet packages are recommended.** The design is achievable with pure HTML/CSS/SVG rendered by Blazor. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` into C# POCOs | | **File I/O** | `System.IO` | Built into .NET 8 | Read `data.json` from disk | | **Configuration** | `IConfiguration` / direct file read | Built into .NET 8 | Optionally expose `data.json` path via `appsettings.json` | **Why not `Newtonsoft.Json`?** `System.Text.Json` is the default in .NET 8, faster, and has no external dependency. For a simple flat JSON structure, it's more than sufficient. | Layer | Technology | Notes | |-------|-----------|-------| | **Primary Store** | `data.json` flat file | Human-editable, no database server needed | | **Schema** | C# POCO classes | `DashboardData`, `Milestone`, `HeatmapRow`, `WorkItem` | **No SQLite, no LiteDB, no Entity Framework.** A JSON file is the right tool for this job. The data is small (dozens of items), read-only at runtime, and edited by hand between screenshot sessions. | Component | Path | Purpose | |-----------|------|---------| | **Solution file** | `ReportingDashboard.sln` | Standard .NET solution | | **Web project** | `src/ReportingDashboard/ReportingDashboard.csproj` | Blazor Server app | | **Data models** | `src/ReportingDashboard/Models/` | POCO classes for JSON deserialization | | **Services** | `src/ReportingDashboard/Services/DashboardDataService.cs` | Reads and deserializes `data.json` | | **Components** | `src/ReportingDashboard/Components/` | Razor components for timeline, heatmap, header | | **Main page** | `src/ReportingDashboard/Components/Pages/Dashboard.razor` | Single-page dashboard | | **Static data** | `src/ReportingDashboard/wwwroot/data/data.json` | Project data file | | **CSS** | `src/ReportingDashboard/wwwroot/css/dashboard.css` | Styles ported from HTML reference | | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Unit Tests** | xUnit | 2.7+ | Test JSON deserialization, data model mapping | | **Component Tests** | bUnit | 1.25+ | Optional: verify Razor component rendering | Testing is low-priority for this utility project. If included, focus on verifying `data.json` parsing doesn't break when the schema evolves. --- This is NOT a CRUD application. Do not apply DDD, CQRS, Clean Architecture, or repository patterns. The architecture should be:
```
data.json → DashboardDataService → Blazor Components → Rendered HTML/SVG/CSS
``` That's it. Three layers, one direction, no mutations.
- **Startup:** `DashboardDataService` is registered as a **Singleton** in DI.
- **On first request:** Service reads `wwwroot/data/data.json` (or a configurable path) and deserializes it into a `DashboardData` object. Cache it in memory.
- **Component rendering:** `Dashboard.razor` injects the service, accesses the data, and renders:
- **Header:** Title, subtitle, legend (static layout + data-driven text)
- **Timeline:** SVG element with data-bound milestone positions, month gridlines, and "NOW" marker
- **Heatmap:** CSS Grid with data-bound rows (Shipped, In Progress, Carryover, Blockers) × month columns
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentMonth": "Apr 2026",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "id": "M1",
      "name": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Feature A", "Feature B"],
        "Feb": ["Feature C"],
        "Mar": ["Feature D", "Feature E"],
        "Apr": ["Feature F"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { ... }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { ... }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { ... }
    }
  ]
}
``` The timeline SVG should be generated in Razor, not with a charting library. Key calculations:
- **X-axis mapping:** Map dates to pixel positions. Given the 1920px viewport and ~1560px SVG width (after left label area), each month spans ~260px.
- **Element types:** Circles for checkpoints (`<circle>`), diamonds for milestones (`<polygon>` with 4 points), colored horizontal lines for swimlanes (`<line>`).
- **"NOW" marker:** Vertical dashed red line positioned by calculating the current date's x-position.
- **All positioning math** should be in the `DashboardDataService` or a small helper, not in the Razor template. Port the CSS from `OriginalDesignConcept.html` directly into `dashboard.css`. Key decisions:
- **Fixed viewport:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` — this is intentional for screenshot fidelity.
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(N, 1fr)` where N is the number of months (data-driven).
- **CSS variables for theming:** Extract the color palette into CSS custom properties for easy rebranding:
  ```css
  :root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    /* ... etc ... */
  }
  ```
- **No CSS framework (Bootstrap, Tailwind, MudBlazor).** The design is custom and specific. A CSS framework would add weight and fight the fixed-layout approach. Keep it simple — 3-4 components maximum: | Component | Responsibility | |-----------|---------------| | `Dashboard.razor` | Main page, injects data service, composes sub-components | | `TimelineSection.razor` | Renders the SVG timeline with milestones and markers | | `HeatmapGrid.razor` | Renders the CSS Grid heatmap with status rows | | `DashboardHeader.razor` | Title, subtitle, backlog link, legend | Each component receives its data as `[Parameter]` properties. No cascading values, no state management, no event callbacks. ---

### Considerations & Risks

- **None.** This is explicitly a local-only tool with no auth requirements. Do not add Identity, cookie auth, or any auth middleware. The `Program.cs` should be minimal:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```
- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of source control via `.gitignore`. | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` from command line or VS Code/Visual Studio | | **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | None — run locally, take screenshots, close the app | | **Containerization** | Not needed for a local utility | Since the primary output is PowerPoint screenshots:
- Run `dotnet run` from the project directory
- Open `http://localhost:5000` in a browser at 1920×1080 (or use Chrome DevTools device toolbar to force resolution)
- Take screenshot (Windows: `Win+Shift+S`, or browser DevTools `Ctrl+Shift+P` → "Capture full size screenshot")
- Paste into PowerPoint **Pro tip:** Add a `<meta name="viewport" content="width=1920">` and consider a print-friendly CSS media query if needed. **$0.** This runs on the developer's local machine. No hosting, no cloud services, no databases, no licenses. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | Adding unnecessary libraries (MudBlazor, charting libs) | High | Medium — adds complexity, fights the design | Stick to raw HTML/CSS/SVG. The HTML reference proves it's achievable without libraries. | | Introducing a database | Medium | Medium — unnecessary complexity | Use `data.json`. Period. | | Building CRUD/editing features | Medium | High — scope creep | The data file is edited by hand. The dashboard is read-only. | | Responsive design work | Medium | Low — wastes time | Fixed 1920×1080. No responsive breakpoints. | The timeline SVG is the most complex visual element. Calculating x-positions from dates, handling variable month widths, and positioning overlapping labels requires care. **Mitigation:** Pre-calculate all SVG positions in the service layer. Create a `TimelineLayout` model with absolute pixel coordinates. The Razor template should only bind, not calculate. Blazor Server maintains a SignalR WebSocket connection, which is unnecessary for a read-only dashboard. This adds ~100KB of JS and a persistent connection. **Trade-off accepted:** The overhead is negligible for a local tool. Blazor Server is chosen for the .NET 8 ecosystem mandate, and it works. Blazor Static SSR (available in .NET 8) could eliminate the SignalR connection but still requires the Blazor Server project template. **Alternative within stack:** Use .NET 8's **Static Server-Side Rendering (Static SSR)** mode. In `Dashboard.razor`, use `@rendermode` without `InteractiveServer` to get pure server-rendered HTML with no WebSocket. This is the recommended approach for a read-only dashboard. As users add months or change categories, the JSON schema may need to evolve. **Mitigation:** Use nullable properties in C# POCOs and provide sensible defaults. Include a `schemaVersion` field in `data.json` for future-proofing. The original design is a single HTML file. Blazor adds a project structure, build step, and runtime. The benefit is data-binding from `data.json` — without Blazor, you'd need JavaScript or manual HTML editing. **Verdict:** Blazor is justified. Editing `data.json` and refreshing the browser is a better workflow than editing HTML cells by hand. The project structure overhead is minimal for .NET developers. ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always a fixed window? **Recommendation:** Make it data-driven — render whatever months are in the JSON.
- **Should the "NOW" marker auto-calculate from system date?** The reference hardcodes "Apr 2026." **Recommendation:** Yes, auto-calculate from `DateTime.Now` but allow an override in `data.json` for generating historical snapshots.
- **Will multiple projects need separate dashboards?** If so, support multiple `data.json` files (e.g., `data-project-a.json`) selectable via URL parameter. **Recommendation:** Start with one, add multi-project later if needed.
- **What is the update cadence?** Weekly? Monthly? This affects whether hot-reload of `data.json` (via `FileSystemWatcher`) is worth implementing. **Recommendation:** Skip hot-reload. The user will restart the app or refresh the browser.
- **Should the ADO Backlog link be live?** The reference includes a link to Azure DevOps. If this is for PowerPoint screenshots, links won't work anyway. **Recommendation:** Include the link for when viewing in-browser, but don't build any ADO integration.
- **Color palette — keep the reference colors or customize?** The reference uses Google-style colors (#34A853 green, #F4B400 yellow, #EA4335 red, #0078D4 blue). **Recommendation:** Keep them — they're well-tested for readability and contrast. Expose via CSS variables for easy swaps. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint executive decks.

**Primary Recommendation:** Build a minimal Blazor Server app with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for deserializing `data.json`. No database, no authentication, no external services. The entire app should be ~5-8 files total. Prioritize visual fidelity to the HTML design reference over architectural sophistication — this is a utility tool, not an enterprise platform.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid (`160px repeat(4,1fr)`), Flexbox, inline SVG for the timeline, and the Segoe UI font family. Blazor Server can reproduce this exactly using identical CSS.
- **No database is needed.** A flat `data.json` file is the ideal storage for this use case — it's human-editable, version-controllable, and trivially deserialized with `System.Text.Json`.
- **No charting library is needed.** The timeline is a hand-crafted SVG with lines, circles, diamonds, and text labels. Blazor's Razor syntax can generate SVG elements directly from data bindings — no third-party library required.
- The heatmap grid is pure CSS Grid with colored cells. No canvas, no WebGL, no charting framework. Blazor components map 1:1 to the HTML structure.
- Blazor Server's SignalR connection is irrelevant for this use case (single user, local, read-only), but it works fine and adds no complexity. Blazor Server is simpler to set up than Blazor WebAssembly for a local tool.
- The fixed 1920×1080 viewport is ideal for screenshot capture. No responsive design is needed — in fact, responsive design should be **avoided** to ensure consistent screenshots.
- The entire project can be implemented in a single afternoon. The architecture should reflect this simplicity.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Server-side rendering with Razor components |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Reproduces the exact grid from the HTML reference |
| **Timeline Visualization** | Inline SVG via Razor | N/A | Milestone diamonds, checkpoint circles, month gridlines, "NOW" marker |
| **Font** | Segoe UI (system font) | N/A | Matches design spec; no web font loading needed on Windows |
| **Icons/Shapes** | Pure CSS + SVG | N/A | Diamond markers via `transform:rotate(45deg)`, circles via `border-radius` |

**No additional frontend NuGet packages are recommended.** The design is achievable with pure HTML/CSS/SVG rendered by Blazor.

### Backend (Data Layer)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` into C# POCOs |
| **File I/O** | `System.IO` | Built into .NET 8 | Read `data.json` from disk |
| **Configuration** | `IConfiguration` / direct file read | Built into .NET 8 | Optionally expose `data.json` path via `appsettings.json` |

**Why not `Newtonsoft.Json`?** `System.Text.Json` is the default in .NET 8, faster, and has no external dependency. For a simple flat JSON structure, it's more than sufficient.

### Data Storage

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Primary Store** | `data.json` flat file | Human-editable, no database server needed |
| **Schema** | C# POCO classes | `DashboardData`, `Milestone`, `HeatmapRow`, `WorkItem` |

**No SQLite, no LiteDB, no Entity Framework.** A JSON file is the right tool for this job. The data is small (dozens of items), read-only at runtime, and edited by hand between screenshot sessions.

### Project Structure

| Component | Path | Purpose |
|-----------|------|---------|
| **Solution file** | `ReportingDashboard.sln` | Standard .NET solution |
| **Web project** | `src/ReportingDashboard/ReportingDashboard.csproj` | Blazor Server app |
| **Data models** | `src/ReportingDashboard/Models/` | POCO classes for JSON deserialization |
| **Services** | `src/ReportingDashboard/Services/DashboardDataService.cs` | Reads and deserializes `data.json` |
| **Components** | `src/ReportingDashboard/Components/` | Razor components for timeline, heatmap, header |
| **Main page** | `src/ReportingDashboard/Components/Pages/Dashboard.razor` | Single-page dashboard |
| **Static data** | `src/ReportingDashboard/wwwroot/data/data.json` | Project data file |
| **CSS** | `src/ReportingDashboard/wwwroot/css/dashboard.css` | Styles ported from HTML reference |

### Testing (Minimal)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Unit Tests** | xUnit | 2.7+ | Test JSON deserialization, data model mapping |
| **Component Tests** | bUnit | 1.25+ | Optional: verify Razor component rendering |

Testing is low-priority for this utility project. If included, focus on verifying `data.json` parsing doesn't break when the schema evolves.

---

## 4. Architecture Recommendations

### Pattern: Minimal Single-Page Read-Only Dashboard

This is NOT a CRUD application. Do not apply DDD, CQRS, Clean Architecture, or repository patterns. The architecture should be:

```
data.json → DashboardDataService → Blazor Components → Rendered HTML/SVG/CSS
```

That's it. Three layers, one direction, no mutations.

### Data Flow

1. **Startup:** `DashboardDataService` is registered as a **Singleton** in DI.
2. **On first request:** Service reads `wwwroot/data/data.json` (or a configurable path) and deserializes it into a `DashboardData` object. Cache it in memory.
3. **Component rendering:** `Dashboard.razor` injects the service, accesses the data, and renders:
   - **Header:** Title, subtitle, legend (static layout + data-driven text)
   - **Timeline:** SVG element with data-bound milestone positions, month gridlines, and "NOW" marker
   - **Heatmap:** CSS Grid with data-bound rows (Shipped, In Progress, Carryover, Blockers) × month columns

### Data Model Design (for `data.json`)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentMonth": "Apr 2026",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "id": "M1",
      "name": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Feature A", "Feature B"],
        "Feb": ["Feature C"],
        "Mar": ["Feature D", "Feature E"],
        "Apr": ["Feature F"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { ... }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { ... }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { ... }
    }
  ]
}
```

### SVG Timeline Rendering Strategy

The timeline SVG should be generated in Razor, not with a charting library. Key calculations:

- **X-axis mapping:** Map dates to pixel positions. Given the 1920px viewport and ~1560px SVG width (after left label area), each month spans ~260px.
- **Element types:** Circles for checkpoints (`<circle>`), diamonds for milestones (`<polygon>` with 4 points), colored horizontal lines for swimlanes (`<line>`).
- **"NOW" marker:** Vertical dashed red line positioned by calculating the current date's x-position.
- **All positioning math** should be in the `DashboardDataService` or a small helper, not in the Razor template.

### CSS Strategy

Port the CSS from `OriginalDesignConcept.html` directly into `dashboard.css`. Key decisions:

- **Fixed viewport:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` — this is intentional for screenshot fidelity.
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(N, 1fr)` where N is the number of months (data-driven).
- **CSS variables for theming:** Extract the color palette into CSS custom properties for easy rebranding:
  ```css
  :root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    /* ... etc ... */
  }
  ```
- **No CSS framework (Bootstrap, Tailwind, MudBlazor).** The design is custom and specific. A CSS framework would add weight and fight the fixed-layout approach.

### Component Decomposition

Keep it simple — 3-4 components maximum:

| Component | Responsibility |
|-----------|---------------|
| `Dashboard.razor` | Main page, injects data service, composes sub-components |
| `TimelineSection.razor` | Renders the SVG timeline with milestones and markers |
| `HeatmapGrid.razor` | Renders the CSS Grid heatmap with status rows |
| `DashboardHeader.razor` | Title, subtitle, backlog link, legend |

Each component receives its data as `[Parameter]` properties. No cascading values, no state management, no event callbacks.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only tool with no auth requirements. Do not add Identity, cookie auth, or any auth middleware. The `Program.cs` should be minimal:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### Data Protection

- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of source control via `.gitignore`.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` from command line or VS Code/Visual Studio |
| **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | None — run locally, take screenshots, close the app |
| **Containerization** | Not needed for a local utility |

### Screenshot Workflow

Since the primary output is PowerPoint screenshots:

1. Run `dotnet run` from the project directory
2. Open `http://localhost:5000` in a browser at 1920×1080 (or use Chrome DevTools device toolbar to force resolution)
3. Take screenshot (Windows: `Win+Shift+S`, or browser DevTools `Ctrl+Shift+P` → "Capture full size screenshot")
4. Paste into PowerPoint

**Pro tip:** Add a `<meta name="viewport" content="width=1920">` and consider a print-friendly CSS media query if needed.

### Infrastructure Costs

**$0.** This runs on the developer's local machine. No hosting, no cloud services, no databases, no licenses.

---

## 6. Risks & Trade-offs

### Risk: Over-Engineering

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Adding unnecessary libraries (MudBlazor, charting libs) | High | Medium — adds complexity, fights the design | Stick to raw HTML/CSS/SVG. The HTML reference proves it's achievable without libraries. |
| Introducing a database | Medium | Medium — unnecessary complexity | Use `data.json`. Period. |
| Building CRUD/editing features | Medium | High — scope creep | The data file is edited by hand. The dashboard is read-only. |
| Responsive design work | Medium | Low — wastes time | Fixed 1920×1080. No responsive breakpoints. |

### Risk: SVG Timeline Complexity

The timeline SVG is the most complex visual element. Calculating x-positions from dates, handling variable month widths, and positioning overlapping labels requires care.

**Mitigation:** Pre-calculate all SVG positions in the service layer. Create a `TimelineLayout` model with absolute pixel coordinates. The Razor template should only bind, not calculate.

### Risk: Blazor Server Overhead for a Static Page

Blazor Server maintains a SignalR WebSocket connection, which is unnecessary for a read-only dashboard. This adds ~100KB of JS and a persistent connection.

**Trade-off accepted:** The overhead is negligible for a local tool. Blazor Server is chosen for the .NET 8 ecosystem mandate, and it works. Blazor Static SSR (available in .NET 8) could eliminate the SignalR connection but still requires the Blazor Server project template.

**Alternative within stack:** Use .NET 8's **Static Server-Side Rendering (Static SSR)** mode. In `Dashboard.razor`, use `@rendermode` without `InteractiveServer` to get pure server-rendered HTML with no WebSocket. This is the recommended approach for a read-only dashboard.

### Risk: `data.json` Schema Evolution

As users add months or change categories, the JSON schema may need to evolve.

**Mitigation:** Use nullable properties in C# POCOs and provide sensible defaults. Include a `schemaVersion` field in `data.json` for future-proofing.

### Trade-off: Blazor vs. Raw HTML

The original design is a single HTML file. Blazor adds a project structure, build step, and runtime. The benefit is data-binding from `data.json` — without Blazor, you'd need JavaScript or manual HTML editing.

**Verdict:** Blazor is justified. Editing `data.json` and refreshing the browser is a better workflow than editing HTML cells by hand. The project structure overhead is minimal for .NET developers.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always a fixed window? **Recommendation:** Make it data-driven — render whatever months are in the JSON.

2. **Should the "NOW" marker auto-calculate from system date?** The reference hardcodes "Apr 2026." **Recommendation:** Yes, auto-calculate from `DateTime.Now` but allow an override in `data.json` for generating historical snapshots.

3. **Will multiple projects need separate dashboards?** If so, support multiple `data.json` files (e.g., `data-project-a.json`) selectable via URL parameter. **Recommendation:** Start with one, add multi-project later if needed.

4. **What is the update cadence?** Weekly? Monthly? This affects whether hot-reload of `data.json` (via `FileSystemWatcher`) is worth implementing. **Recommendation:** Skip hot-reload. The user will restart the app or refresh the browser.

5. **Should the ADO Backlog link be live?** The reference includes a link to Azure DevOps. If this is for PowerPoint screenshots, links won't work anyway. **Recommendation:** Include the link for when viewing in-browser, but don't build any ADO integration.

6. **Color palette — keep the reference colors or customize?** The reference uses Google-style colors (#34A853 green, #F4B400 yellow, #EA4335 red, #0078D4 blue). **Recommendation:** Keep them — they're well-tested for readability and contrast. Expose via CSS variables for easy swaps.

---

## 8. Implementation Recommendations

### Phase 1: Skeleton + CSS (2-3 hours)

1. Create the .NET 8 Blazor Server solution (`dotnet new blazor --name ReportingDashboard --interactivity Server`)
2. Port the CSS from `OriginalDesignConcept.html` into `dashboard.css`
3. Create the `Dashboard.razor` page with hardcoded HTML matching the reference
4. Verify pixel-perfect rendering at 1920×1080

**Milestone:** Static HTML dashboard renders identically to the reference design.

### Phase 2: Data Model + JSON Binding (1-2 hours)

1. Define C# POCO models: `DashboardData`, `Milestone`, `MilestoneEvent`, `StatusCategory`, `MonthItems`
2. Create `data.json` with fictional project data
3. Build `DashboardDataService` to deserialize `data.json`
4. Replace hardcoded HTML with Razor data bindings (`@foreach`, `@if`, string interpolation)

**Milestone:** Dashboard renders from `data.json`. Changing the JSON changes the display.

### Phase 3: SVG Timeline (1-2 hours)

1. Build `TimelineSection.razor` component
2. Implement date-to-pixel mapping utility
3. Render milestone swimlanes, markers (circles, diamonds), month gridlines, "NOW" line
4. Test with different date ranges to ensure layout doesn't break

**Milestone:** Timeline accurately plots milestones from JSON data.

### Phase 4: Polish + Screenshot Optimization (1 hour)

1. Fine-tune spacing, font sizes, colors to match reference
2. Add CSS custom properties for easy re-theming
3. Test screenshot workflow (browser → screenshot → PowerPoint)
4. Add a `README.md` explaining how to edit `data.json` and take screenshots

**Milestone:** Production-ready screenshot utility.

### Quick Wins

- **CSS custom properties** for color theming — swap project branding in seconds
- **Static SSR mode** — eliminates the SignalR connection, faster page load, simpler
- **`dotnet watch`** during development — auto-refreshes when `data.json` or Razor files change
- **Browser DevTools device mode** — force exact 1920×1080 viewport without resizing the window

### What NOT to Build

- ❌ Authentication or user management
- ❌ Database or ORM layer
- ❌ REST API endpoints
- ❌ Real-time updates or SignalR push
- ❌ Responsive/mobile layout
- ❌ Admin panel for editing data
- ❌ PDF export (use screenshots instead)
- ❌ Caching layer (data is read once, it's tiny)
- ❌ Logging framework (console output is fine)
- ❌ Docker containerization
- ❌ CI/CD pipeline

### Estimated Total Effort

**5-8 hours** for a developer familiar with Blazor, or **1-2 days** for someone learning Blazor basics. The HTML reference already solves the hardest problem (visual design), so the implementation is primarily a porting exercise with data-binding added.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/9e282b50d98e485df523eb74e931a1ae8b72774b/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
