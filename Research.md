# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 14:20 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and monthly heatmaps. The dashboard reads from a local `data.json` file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint decks. **Primary Recommendation:** Keep this brutally simple. Use a single Blazor Server project with zero external UI component libraries. The original HTML design is already well-structured with pure CSS Grid, Flexbox, and inline SVG — all of which Blazor can render natively. The only NuGet dependency beyond the framework should be `System.Text.Json` (built-in) for deserializing `data.json`. No database. No auth. No API layer. One `.razor` page, one data model, one JSON file. This approach maximizes maintainability, minimizes build complexity, and ensures any developer can pick it up in minutes. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | Yes (implicit) | | `System.Text.Json` | 8.0.x | JSON deserialization | Yes (built-in, no install) | | `xunit` | 2.7.x | Unit testing | Optional | | `bUnit` | 1.28.x | Component testing | Optional | | `Microsoft.Playwright` | 1.41.x | Screenshot automation | Optional | From the original design, preserve these exactly: | Usage | Hex | Context | |-------|-----|---------| | Background | `#FFFFFF` | Page body | | Text primary | `#111` | Headings | | Text secondary | `#888` | Subtitles, section labels | | Link / accent | `#0078D4` | URLs, M1 track, In Progress bullets | | Grid border | `#E0E0E0` | Cell borders | | Header row bg | `#F5F5F5` | Column headers | | Current month header bg | `#FFF0D0` | Highlighted month | | Current month header text | `#C07700` | Highlighted month label | | Shipped green | `#34A853` | Bullets, production diamonds | | Shipped cell bg | `#F0FBF0` / `#D8F2DA` | Normal / current month | | Shipped header bg | `#E8F5E9` | Row header | | In Progress blue | `#0078D4` | Bullets | | In Progress cell bg | `#EEF4FE` / `#DAE8FB` | Normal / current month | | Carryover amber | `#F4B400` | Bullets, PoC diamonds | | Carryover cell bg | `#FFFDE7` / `#FFF0B0` | Normal / current month | | Blocker red | `#EA4335` | Bullets, NOW line | | Blocker cell bg | `#FFF5F5` / `#FFE4E4` | Normal / current month |

### Key Findings

- **The original HTML design uses only CSS Grid, Flexbox, and inline SVG** — no charting library is needed. Blazor can render all of this natively without any JavaScript interop or third-party component libraries.
- **A JSON flat-file (`data.json`) is the ideal data store** for this use case. There are no queries, no concurrent writes, no relational joins. SQLite or any database would be over-engineering.
- **Blazor Server is well-suited** for local-only, single-user dashboards. The SignalR circuit overhead is negligible for one user, and Server-side rendering means zero WASM download time.
- **No authentication or authorization is needed** per requirements. The app runs locally, likely on `localhost:5000`, and is never exposed to the internet.
- **The SVG timeline is the most complex rendering piece.** It requires calculating pixel positions from date ranges. This should be a dedicated Blazor component with pure C# math — no JS charting library.
- **Screenshot fidelity is the #1 UX requirement.** The page must render at exactly 1920×1080 with no scrollbars, no loading spinners, and no interactive state that could interfere with a clean capture.
- **Existing open-source alternatives** (Grafana, Metabase, PowerBI) are all overkill for this use case. They require databases, authentication, and produce opinionated designs that don't match the custom HTML spec.
- **The "current month" highlight pattern** (amber column headers, darker cell backgrounds) must be data-driven from `data.json`, not hardcoded to April. --- **Goal:** Render the heatmap grid with fake data from `data.json`.
- `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
- Create `data.json` with sample data for a fictional project (e.g., "Project Phoenix — Cloud Migration").
- Create `DashboardData.cs` model and `DashboardDataService.cs`.
- Port CSS from original HTML into `dashboard.css`.
- Build `Dashboard.razor` with inline heatmap grid (no sub-components yet).
- Verify 1920×1080 screenshot matches the original design's heatmap section. **Deliverable:** Working heatmap grid, correct colors, correct layout, data-driven from JSON. **Goal:** Add the SVG milestone timeline above the heatmap.
- Extract `Timeline.razor` component.
- Implement date-to-pixel math in C#.
- Render milestone tracks, diamonds, circles, and the "NOW" line.
- Verify against the original design's timeline section. **Deliverable:** Full page matches the original HTML design with real data-driven rendering. **Goal:** Improve on the original design for executive readability.
- **Subtle animations:** Fade-in on page load (CSS only, no JS).
- **Better typography:** Slightly larger heatmap item text (13px vs 12px) for readability on projected screens.
- **Empty state handling:** Show "—" or light text when a cell has no items.
- **Tooltip on hover** (optional): Show full item name if truncated. Use CSS `title` attribute — no JS needed.
- **data.sample.json:** Well-commented template file for users to copy.
- **Instant feedback loop:** `dotnet watch run` gives hot reload on Razor and CSS changes — iterate visually in real time.
- **Browser DevTools screenshot:** Chrome DevTools → `Ctrl+Shift+P` → "Capture full size screenshot" at 1920×1080 — no external tool needed.
- **Color constants in C#:** Define color hex values as constants in a `ThemeColors` static class to keep Razor markup clean.
- ❌ Authentication / authorization
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Admin panel / data editor UI
- ❌ Responsive / mobile layout
- ❌ Dark mode
- ❌ Export to PDF/Excel
- ❌ Real-time updates / SignalR push
- ❌ Multi-project support (one JSON = one project = one page)
- ❌ CI/CD pipeline (local tool, not deployed) ---

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | LTS release, supported through Nov 2026 | | **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches the original HTML design exactly | | **SVG Rendering** | Inline SVG via Razor markup | N/A | No charting library — hand-authored SVG like the original | | **Fonts** | Segoe UI (system font) | N/A | Already available on Windows; fallback to Arial | | **Icons/Shapes** | CSS transforms + SVG polygons | N/A | Diamond milestones use `transform:rotate(45deg)` on `<span>` or SVG `<polygon>` | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Data Source** | `data.json` flat file | N/A | Loaded at startup via `IConfiguration` or direct `File.ReadAllText` | | **Serialization** | `System.Text.Json` | Built into .NET 8 | No NuGet install needed. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` | | **Data Service** | Singleton `DashboardDataService` | N/A | Reads and deserializes JSON once, exposes strongly-typed model | | **Hot Reload** | `IFileProvider` + `FileSystemWatcher` | Built-in | Optional: watch `data.json` for changes so edits reflect without restart | | Layer | Technology | Notes | |-------|-----------|-------| | **Solution** | `.sln` with single project | `ReportingDashboard.sln` → `ReportingDashboard.csproj` | | **Project Template** | `blazorserver` | `dotnet new blazorserver -n ReportingDashboard --framework net8.0` | | **CSS Strategy** | Single `dashboard.css` in `wwwroot/css/` | Ported directly from the HTML design's `<style>` block | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7.x | For data model deserialization and date-to-pixel math | | **Component Tests** | bUnit | 1.28.x+ | For verifying Razor component output (optional — low priority given simplicity) | | **Snapshot Testing** | Playwright for .NET | 1.41+ | Optional: automate 1920×1080 screenshots for regression | | Tool | Version | Purpose | |------|---------|---------| | .NET SDK | 8.0.x (latest patch) | Build and run | | Visual Studio 2022 | 17.9+ | IDE with Blazor hot reload | | VS Code + C# Dev Kit | Latest | Lightweight alternative | --- This is a one-page app. Do not over-architect it.
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── data.json                          ← project data (copied to output)
│   ├── Models/
│   │   └── DashboardData.cs               ← strongly-typed JSON model
│   ├── Services/
│   │   └── DashboardDataService.cs        ← reads & deserializes data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor            ← the single page (route: "/")
│   │   ├── Layout/
│   │   │   └── DashboardLayout.razor      ← minimal layout (no nav, no sidebar)
│   │   ├── Header.razor                   ← title, subtitle, legend
│   │   ├── Timeline.razor                 ← SVG milestone visualization
│   │   ├── Heatmap.razor                  ← CSS Grid status matrix
│   │   └── HeatmapCell.razor              ← individual cell with item bullets
│   └── wwwroot/
│       └── css/
│           └── dashboard.css              ← all styles from original HTML
```
```
data.json → DashboardDataService (Singleton) → Dashboard.razor → Child Components
```
- `Program.cs` registers `DashboardDataService` as a singleton.
- `DashboardDataService` reads `data.json` from disk on first access (lazy or eager).
- `Dashboard.razor` injects the service and passes data to child components via `[Parameter]`.
- No API controllers, no HTTP calls, no database, no state management library.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-17",
  "currentMonthLabel": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
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
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": ["Item D", "Item E"],
        "Apr": ["Item F"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { "Jan": [], "Feb": ["Item G"], "Mar": ["Item H"], "Apr": ["Item I", "Item J"] }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { "Jan": [], "Feb": [], "Mar": ["Item K"], "Apr": ["Item L"] }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item M"] }
    }
  ]
}
``` The timeline is the most technically interesting piece. Approach:
- **Define a viewport**: The SVG has a fixed `width` (e.g., 1560px as in original) and `height` (185px).
- **Map dates to X-coordinates**: `xPos = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth`
- **Render in C#**: Use a `@foreach` loop over milestones, computing positions in the `@code` block.
- **Shape types**: Checkpoint = `<circle>`, PoC = `<polygon>` (diamond, fill `#F4B400`), Production = `<polygon>` (diamond, fill `#34A853`).
- **"NOW" line**: Vertical dashed line at `currentDate`'s X position, red (#EA4335). All math is trivial C# — no JS interop or charting library needed. Port the original HTML's `<style>` block verbatim into `wwwroot/css/dashboard.css`. Key patterns to preserve:
- **Fixed viewport**: `body { width: 1920px; height: 1080px; overflow: hidden; }` — critical for screenshot fidelity.
- **CSS Grid for heatmap**: `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months (data-driven).
- **Color-coded rows**: `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` with distinct background colors.
- **Current month highlight**: `.apr` class (or dynamic equivalent) for darker cell backgrounds and amber header. **Dynamic column count**: The original hardcodes 4 months. Make this data-driven by setting `grid-template-columns` via a `style` attribute in Razor: `style="grid-template-columns: 160px repeat(@Model.Months.Count, 1fr)"`. ---

### Considerations & Risks

- **None.** Per requirements, this is a local-only tool with no auth. The app binds to `localhost` only. In `Program.cs` or `launchSettings.json`:
```json
"applicationUrl": "http://localhost:5000"
``` Do **not** add HTTPS for local-only use — it adds certificate management complexity for zero benefit.
- `data.json` sits on the local filesystem. No encryption needed — it contains project status data, not secrets.
- If the JSON ever contains sensitive info (employee names, cost data), consider marking the file with appropriate NTFS permissions, but this is outside the app's scope. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Self-contained .NET 8 publish (`dotnet publish -c Release --self-contained -r win-x64`) | | **Hosting** | Kestrel on localhost, no reverse proxy needed | | **Distribution** | Zip the publish output; user runs the `.exe` directly | | **Container** | Not needed for a single-user local tool | | **Cost** | $0 — runs on the user's existing workstation |
- **No monitoring needed** — single user, local only.
- **No logging framework** — `Console.WriteLine` or default `ILogger` to console is sufficient for debugging.
- **Graceful degradation**: If `data.json` is missing or malformed, show a clear error message in the UI rather than a stack trace. --- The timeline's date-to-pixel math and overlapping label placement could consume disproportionate development time. **Mitigation:** Start with a simplified timeline (fixed month boundaries, no overlapping label detection). Iterate on label positioning after the heatmap grid is working. Accept minor label overlaps in v1 — the user can adjust dates in `data.json` to avoid collisions. The design targets a fixed resolution for screenshot capture. Developers with smaller screens may see horizontal scrollbars during editing. **Mitigation:** This is acceptable — the primary output is screenshots, not interactive use. Add `overflow: auto` on `body` for development, switch to `overflow: hidden` for captures. Or use browser zoom (Ctrl+minus) during development. Blazor Server maintains a SignalR connection, which is unnecessary for what is essentially a static page. **Mitigation:** This is a non-issue for single-user local use. The SignalR circuit adds ~50KB overhead and negligible CPU. The benefit of Blazor Server (instant rendering, no WASM download, C# everywhere) outweighs the theoretical overhead. If this ever becomes a concern, the same Razor components can be compiled to static HTML with minimal changes. As users edit `data.json` by hand, they may introduce typos or structural errors. **Mitigation:** Add a `JsonSchema` validation step on startup using `System.Text.Json` deserialization with strict options. Provide a clear, user-friendly error page listing what's wrong. Include a well-commented `data.sample.json` as a template. Using no component library means writing CSS by hand, but this is the right call because:
- The design is fixed and specific — a component library's opinions would fight the design.
- The CSS is already written in the HTML reference — it just needs porting.
- Zero dependency risk, zero version conflicts, zero learning curve.
- Total CSS is ~120 lines — this is not a burden. A JSON file is correct here because:
- Data changes monthly (low frequency).
- One user, no concurrency.
- No querying needed — the entire dataset is loaded and rendered.
- Users can edit JSON in any text editor — lower barrier than SQLite tooling. ---
- **How many months should be visible?** The original shows 4 months (Jan–Apr). Should this be configurable in `data.json`? **Recommendation:** Yes, make it data-driven. The grid columns adapt via `repeat(N, 1fr)`.
- **Should the "current month" highlight be automatic or manual?** Option A: Compute from system date. Option B: Explicit `currentMonthLabel` in JSON. **Recommendation:** Use `currentDate` in JSON and derive the highlight — this allows generating screenshots for past/future months.
- **How will users edit `data.json`?** Direct text editor? Or should the app include a simple edit form? **Recommendation:** Text editor only for v1. An edit form is a significant scope increase for minimal value — executives won't edit the data themselves.
- **Should the timeline support more than 3 milestone tracks?** The original has M1, M2, M3. **Recommendation:** Make it data-driven with no hardcoded limit, but optimize the layout for 2–5 tracks.
- **Print/PDF export needed?** If screenshots aren't sufficient, browser print-to-PDF at 1920×1080 works well. No additional library needed.
- **Will this eventually need to pull from ADO/Jira APIs?** If so, the `DashboardDataService` abstraction makes it easy to swap the data source later without changing any components. But do not build this now. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and monthly heatmaps. The dashboard reads from a local `data.json` file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint decks.

**Primary Recommendation:** Keep this brutally simple. Use a single Blazor Server project with zero external UI component libraries. The original HTML design is already well-structured with pure CSS Grid, Flexbox, and inline SVG — all of which Blazor can render natively. The only NuGet dependency beyond the framework should be `System.Text.Json` (built-in) for deserializing `data.json`. No database. No auth. No API layer. One `.razor` page, one data model, one JSON file.

This approach maximizes maintainability, minimizes build complexity, and ensures any developer can pick it up in minutes.

---

## 2. Key Findings

- **The original HTML design uses only CSS Grid, Flexbox, and inline SVG** — no charting library is needed. Blazor can render all of this natively without any JavaScript interop or third-party component libraries.
- **A JSON flat-file (`data.json`) is the ideal data store** for this use case. There are no queries, no concurrent writes, no relational joins. SQLite or any database would be over-engineering.
- **Blazor Server is well-suited** for local-only, single-user dashboards. The SignalR circuit overhead is negligible for one user, and Server-side rendering means zero WASM download time.
- **No authentication or authorization is needed** per requirements. The app runs locally, likely on `localhost:5000`, and is never exposed to the internet.
- **The SVG timeline is the most complex rendering piece.** It requires calculating pixel positions from date ranges. This should be a dedicated Blazor component with pure C# math — no JS charting library.
- **Screenshot fidelity is the #1 UX requirement.** The page must render at exactly 1920×1080 with no scrollbars, no loading spinners, and no interactive state that could interfere with a clean capture.
- **Existing open-source alternatives** (Grafana, Metabase, PowerBI) are all overkill for this use case. They require databases, authentication, and produce opinionated designs that don't match the custom HTML spec.
- **The "current month" highlight pattern** (amber column headers, darker cell backgrounds) must be data-driven from `data.json`, not hardcoded to April.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | LTS release, supported through Nov 2026 |
| **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches the original HTML design exactly |
| **SVG Rendering** | Inline SVG via Razor markup | N/A | No charting library — hand-authored SVG like the original |
| **Fonts** | Segoe UI (system font) | N/A | Already available on Windows; fallback to Arial |
| **Icons/Shapes** | CSS transforms + SVG polygons | N/A | Diamond milestones use `transform:rotate(45deg)` on `<span>` or SVG `<polygon>` |

### Backend / Data Layer

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Data Source** | `data.json` flat file | N/A | Loaded at startup via `IConfiguration` or direct `File.ReadAllText` |
| **Serialization** | `System.Text.Json` | Built into .NET 8 | No NuGet install needed. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` |
| **Data Service** | Singleton `DashboardDataService` | N/A | Reads and deserializes JSON once, exposes strongly-typed model |
| **Hot Reload** | `IFileProvider` + `FileSystemWatcher` | Built-in | Optional: watch `data.json` for changes so edits reflect without restart |

### Project Structure

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Solution** | `.sln` with single project | `ReportingDashboard.sln` → `ReportingDashboard.csproj` |
| **Project Template** | `blazorserver` | `dotnet new blazorserver -n ReportingDashboard --framework net8.0` |
| **CSS Strategy** | Single `dashboard.css` in `wwwroot/css/` | Ported directly from the HTML design's `<style>` block |

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7.x | For data model deserialization and date-to-pixel math |
| **Component Tests** | bUnit | 1.28.x+ | For verifying Razor component output (optional — low priority given simplicity) |
| **Snapshot Testing** | Playwright for .NET | 1.41+ | Optional: automate 1920×1080 screenshots for regression |

### Development Tools

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 8.0.x (latest patch) | Build and run |
| Visual Studio 2022 | 17.9+ | IDE with Blazor hot reload |
| VS Code + C# Dev Kit | Latest | Lightweight alternative |

---

## 4. Architecture Recommendations

### Pattern: **Single-Component Page with Sub-Components**

This is a one-page app. Do not over-architect it.

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── data.json                          ← project data (copied to output)
│   ├── Models/
│   │   └── DashboardData.cs               ← strongly-typed JSON model
│   ├── Services/
│   │   └── DashboardDataService.cs        ← reads & deserializes data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor            ← the single page (route: "/")
│   │   ├── Layout/
│   │   │   └── DashboardLayout.razor      ← minimal layout (no nav, no sidebar)
│   │   ├── Header.razor                   ← title, subtitle, legend
│   │   ├── Timeline.razor                 ← SVG milestone visualization
│   │   ├── Heatmap.razor                  ← CSS Grid status matrix
│   │   └── HeatmapCell.razor              ← individual cell with item bullets
│   └── wwwroot/
│       └── css/
│           └── dashboard.css              ← all styles from original HTML
```

### Data Flow

```
data.json → DashboardDataService (Singleton) → Dashboard.razor → Child Components
```

1. `Program.cs` registers `DashboardDataService` as a singleton.
2. `DashboardDataService` reads `data.json` from disk on first access (lazy or eager).
3. `Dashboard.razor` injects the service and passes data to child components via `[Parameter]`.
4. No API controllers, no HTTP calls, no database, no state management library.

### Data Model Design (`data.json`)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-17",
  "currentMonthLabel": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
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
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": ["Item D", "Item E"],
        "Apr": ["Item F"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { "Jan": [], "Feb": ["Item G"], "Mar": ["Item H"], "Apr": ["Item I", "Item J"] }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { "Jan": [], "Feb": [], "Mar": ["Item K"], "Apr": ["Item L"] }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item M"] }
    }
  ]
}
```

### SVG Timeline Rendering Strategy

The timeline is the most technically interesting piece. Approach:

1. **Define a viewport**: The SVG has a fixed `width` (e.g., 1560px as in original) and `height` (185px).
2. **Map dates to X-coordinates**: `xPos = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth`
3. **Render in C#**: Use a `@foreach` loop over milestones, computing positions in the `@code` block.
4. **Shape types**: Checkpoint = `<circle>`, PoC = `<polygon>` (diamond, fill `#F4B400`), Production = `<polygon>` (diamond, fill `#34A853`).
5. **"NOW" line**: Vertical dashed line at `currentDate`'s X position, red (#EA4335).

All math is trivial C# — no JS interop or charting library needed.

### CSS Strategy

Port the original HTML's `<style>` block verbatim into `wwwroot/css/dashboard.css`. Key patterns to preserve:

- **Fixed viewport**: `body { width: 1920px; height: 1080px; overflow: hidden; }` — critical for screenshot fidelity.
- **CSS Grid for heatmap**: `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months (data-driven).
- **Color-coded rows**: `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` with distinct background colors.
- **Current month highlight**: `.apr` class (or dynamic equivalent) for darker cell backgrounds and amber header.

**Dynamic column count**: The original hardcodes 4 months. Make this data-driven by setting `grid-template-columns` via a `style` attribute in Razor: `style="grid-template-columns: 160px repeat(@Model.Months.Count, 1fr)"`.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** Per requirements, this is a local-only tool with no auth. The app binds to `localhost` only.

In `Program.cs` or `launchSettings.json`:
```json
"applicationUrl": "http://localhost:5000"
```

Do **not** add HTTPS for local-only use — it adds certificate management complexity for zero benefit.

### Data Protection

- `data.json` sits on the local filesystem. No encryption needed — it contains project status data, not secrets.
- If the JSON ever contains sensitive info (employee names, cost data), consider marking the file with appropriate NTFS permissions, but this is outside the app's scope.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Self-contained .NET 8 publish (`dotnet publish -c Release --self-contained -r win-x64`) |
| **Hosting** | Kestrel on localhost, no reverse proxy needed |
| **Distribution** | Zip the publish output; user runs the `.exe` directly |
| **Container** | Not needed for a single-user local tool |
| **Cost** | $0 — runs on the user's existing workstation |

### Operational Concerns

- **No monitoring needed** — single user, local only.
- **No logging framework** — `Console.WriteLine` or default `ILogger` to console is sufficient for debugging.
- **Graceful degradation**: If `data.json` is missing or malformed, show a clear error message in the UI rather than a stack trace.

---

## 6. Risks & Trade-offs

### Risk: SVG Timeline Complexity

**Severity: Medium** | **Likelihood: Medium**

The timeline's date-to-pixel math and overlapping label placement could consume disproportionate development time.

**Mitigation:** Start with a simplified timeline (fixed month boundaries, no overlapping label detection). Iterate on label positioning after the heatmap grid is working. Accept minor label overlaps in v1 — the user can adjust dates in `data.json` to avoid collisions.

### Risk: Fixed 1920×1080 Layout Doesn't Fit All Screens

**Severity: Low** | **Likelihood: Medium**

The design targets a fixed resolution for screenshot capture. Developers with smaller screens may see horizontal scrollbars during editing.

**Mitigation:** This is acceptable — the primary output is screenshots, not interactive use. Add `overflow: auto` on `body` for development, switch to `overflow: hidden` for captures. Or use browser zoom (Ctrl+minus) during development.

### Risk: Blazor Server Overhead for a Static Page

**Severity: Low** | **Likelihood: Low**

Blazor Server maintains a SignalR connection, which is unnecessary for what is essentially a static page.

**Mitigation:** This is a non-issue for single-user local use. The SignalR circuit adds ~50KB overhead and negligible CPU. The benefit of Blazor Server (instant rendering, no WASM download, C# everywhere) outweighs the theoretical overhead. If this ever becomes a concern, the same Razor components can be compiled to static HTML with minimal changes.

### Risk: `data.json` Schema Drift

**Severity: Low** | **Likelihood: Medium**

As users edit `data.json` by hand, they may introduce typos or structural errors.

**Mitigation:** Add a `JsonSchema` validation step on startup using `System.Text.Json` deserialization with strict options. Provide a clear, user-friendly error page listing what's wrong. Include a well-commented `data.sample.json` as a template.

### Trade-off: No Component Library vs. MudBlazor/Radzen

Using no component library means writing CSS by hand, but this is the right call because:
1. The design is fixed and specific — a component library's opinions would fight the design.
2. The CSS is already written in the HTML reference — it just needs porting.
3. Zero dependency risk, zero version conflicts, zero learning curve.
4. Total CSS is ~120 lines — this is not a burden.

### Trade-off: No Database vs. SQLite

A JSON file is correct here because:
1. Data changes monthly (low frequency).
2. One user, no concurrency.
3. No querying needed — the entire dataset is loaded and rendered.
4. Users can edit JSON in any text editor — lower barrier than SQLite tooling.

---

## 7. Open Questions

1. **How many months should be visible?** The original shows 4 months (Jan–Apr). Should this be configurable in `data.json`? **Recommendation:** Yes, make it data-driven. The grid columns adapt via `repeat(N, 1fr)`.

2. **Should the "current month" highlight be automatic or manual?** Option A: Compute from system date. Option B: Explicit `currentMonthLabel` in JSON. **Recommendation:** Use `currentDate` in JSON and derive the highlight — this allows generating screenshots for past/future months.

3. **How will users edit `data.json`?** Direct text editor? Or should the app include a simple edit form? **Recommendation:** Text editor only for v1. An edit form is a significant scope increase for minimal value — executives won't edit the data themselves.

4. **Should the timeline support more than 3 milestone tracks?** The original has M1, M2, M3. **Recommendation:** Make it data-driven with no hardcoded limit, but optimize the layout for 2–5 tracks.

5. **Print/PDF export needed?** If screenshots aren't sufficient, browser print-to-PDF at 1920×1080 works well. No additional library needed.

6. **Will this eventually need to pull from ADO/Jira APIs?** If so, the `DashboardDataService` abstraction makes it easy to swap the data source later without changing any components. But do not build this now.

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Render the heatmap grid with fake data from `data.json`.

1. `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
2. Create `data.json` with sample data for a fictional project (e.g., "Project Phoenix — Cloud Migration").
3. Create `DashboardData.cs` model and `DashboardDataService.cs`.
4. Port CSS from original HTML into `dashboard.css`.
5. Build `Dashboard.razor` with inline heatmap grid (no sub-components yet).
6. Verify 1920×1080 screenshot matches the original design's heatmap section.

**Deliverable:** Working heatmap grid, correct colors, correct layout, data-driven from JSON.

### Phase 2: Timeline (1 day)

**Goal:** Add the SVG milestone timeline above the heatmap.

1. Extract `Timeline.razor` component.
2. Implement date-to-pixel math in C#.
3. Render milestone tracks, diamonds, circles, and the "NOW" line.
4. Verify against the original design's timeline section.

**Deliverable:** Full page matches the original HTML design with real data-driven rendering.

### Phase 3: Polish & Improvements (0.5–1 day)

**Goal:** Improve on the original design for executive readability.

1. **Subtle animations:** Fade-in on page load (CSS only, no JS).
2. **Better typography:** Slightly larger heatmap item text (13px vs 12px) for readability on projected screens.
3. **Empty state handling:** Show "—" or light text when a cell has no items.
4. **Tooltip on hover** (optional): Show full item name if truncated. Use CSS `title` attribute — no JS needed.
5. **data.sample.json:** Well-commented template file for users to copy.

### Quick Wins

- **Instant feedback loop:** `dotnet watch run` gives hot reload on Razor and CSS changes — iterate visually in real time.
- **Browser DevTools screenshot:** Chrome DevTools → `Ctrl+Shift+P` → "Capture full size screenshot" at 1920×1080 — no external tool needed.
- **Color constants in C#:** Define color hex values as constants in a `ThemeColors` static class to keep Razor markup clean.

### What NOT to Build

- ❌ Authentication / authorization
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Admin panel / data editor UI
- ❌ Responsive / mobile layout
- ❌ Dark mode
- ❌ Export to PDF/Excel
- ❌ Real-time updates / SignalR push
- ❌ Multi-project support (one JSON = one project = one page)
- ❌ CI/CD pipeline (local tool, not deployed)

---

## Appendix A: NuGet Package Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | Yes (implicit) |
| `System.Text.Json` | 8.0.x | JSON deserialization | Yes (built-in, no install) |
| `xunit` | 2.7.x | Unit testing | Optional |
| `bUnit` | 1.28.x | Component testing | Optional |
| `Microsoft.Playwright` | 1.41.x | Screenshot automation | Optional |

**Total required NuGet packages beyond the framework: 0.**

## Appendix B: Color Palette Reference

From the original design, preserve these exactly:

| Usage | Hex | Context |
|-------|-----|---------|
| Background | `#FFFFFF` | Page body |
| Text primary | `#111` | Headings |
| Text secondary | `#888` | Subtitles, section labels |
| Link / accent | `#0078D4` | URLs, M1 track, In Progress bullets |
| Grid border | `#E0E0E0` | Cell borders |
| Header row bg | `#F5F5F5` | Column headers |
| Current month header bg | `#FFF0D0` | Highlighted month |
| Current month header text | `#C07700` | Highlighted month label |
| Shipped green | `#34A853` | Bullets, production diamonds |
| Shipped cell bg | `#F0FBF0` / `#D8F2DA` | Normal / current month |
| Shipped header bg | `#E8F5E9` | Row header |
| In Progress blue | `#0078D4` | Bullets |
| In Progress cell bg | `#EEF4FE` / `#DAE8FB` | Normal / current month |
| Carryover amber | `#F4B400` | Bullets, PoC diamonds |
| Carryover cell bg | `#FFFDE7` / `#FFF0B0` | Normal / current month |
| Blocker red | `#EA4335` | Bullets, NOW line |
| Blocker cell bg | `#FFF5F5` / `#FFE4E4` | Normal / current month |

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/28878086f8868ea8c8fc2126ee59c4bbf0647a4e/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
