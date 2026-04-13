# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-13 22:41 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. It reads project milestone and status data from a `data.json` file and renders a visually polished view designed for screenshot capture at 1920×1080 for PowerPoint decks. The design features three visual zones: a **header with legend**, an **SVG timeline/Gantt with milestone diamonds and checkpoints**, and a **color-coded heatmap grid** (Shipped/In Progress/Carryover/Blockers × months). The entire stack is intentionally simple—no auth, no database server, no enterprise middleware. The primary recommendation is a minimal Blazor Server app with inline SVG rendering, CSS Grid/Flexbox layout, and `System.Text.Json` for config deserialization. Total project complexity: low. Time to MVP: 1–2 days for a skilled .NET developer. ---

### Key Findings

- The original HTML design is fully static with inline CSS—Blazor components can replicate this 1:1 with `.razor` files and scoped CSS isolation.
- SVG timeline rendering does **not** require a charting library; the original design uses hand-crafted `<svg>` elements with `<line>`, `<circle>`, `<polygon>`, and `<text>` which Blazor can emit directly via Razor markup.
- The heatmap grid uses CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`) and does not need a third-party grid component.
- Reading `data.json` at startup via `System.Text.Json` is sufficient; no database, no ORM, no caching layer needed.
- Blazor Server's SignalR circuit is irrelevant for this use case (single local user, no interactivity beyond page load), but it works fine and adds zero complexity.
- The design targets a fixed 1920×1080 viewport—responsive design is explicitly **not** needed; the page should be optimized for screenshot fidelity at that resolution.
- No authentication, authorization, or HTTPS configuration is needed. `dotnet run` on localhost is the deployment model.
- Hot reload (`dotnet watch`) provides excellent developer experience for iterating on layout. --- **Goal:** Pixel-accurate reproduction of the original HTML design, driven by `data.json`.
- **Scaffold project** (30 min)
- `dotnet new blazorserver -n ReportingDashboard`
- Remove default template pages (Counter, Weather, etc.)
- Create `data.json` with fictional project data
- **Build data model** (30 min)
- Create `Models/DashboardData.cs` with POCOs matching JSON schema
- Register a `DashboardDataService` singleton that deserializes `data.json` on startup
- Add `FileSystemWatcher` for live reload when JSON changes
- **Build header component** (30 min)
- `Components/DashboardHeader.razor` — title, subtitle, legend
- Copy CSS directly from original HTML, adapt to Blazor CSS isolation
- **Build timeline/SVG component** (2–3 hours)
- `Components/TimelineSection.razor` — track labels sidebar + SVG box
- `Components/TimelineTrack.razor` — horizontal line with markers
- Date-to-pixel coordinate calculation helper method
- Diamond markers (`<polygon>`), circle markers (`<circle>`), NOW line
- **Build heatmap component** (2–3 hours)
- `Components/HeatmapSection.razor` — CSS Grid container
- `Components/HeatmapRow.razor` — row header + data cells
- Color class mapping from category (shipped/inProgress/carryover/blockers)
- Current-month column highlighting
- **Screenshot validation** (30 min)
- Open at 1920×1080, compare with original design
- Adjust spacing, font sizes, colors until match is satisfactory
- Add Playwright screenshot automation (`dotnet test` generates PNG)
- Add a simple edit form (Blazor form that writes back to `data.json`)
- Add print-friendly CSS (`@media print`)
- Add a second "compact" layout variant for email embedding
- **`dotnet watch` hot reload** — iterate on CSS and layout in real-time with near-instant feedback.
- **Copy-paste the original CSS** — the HTML design's `<style>` block translates almost 1:1 into `.razor.css` files. Don't rewrite it; adapt it.
- **Fictional project data first** — get the visuals right with fake data before wiring up real project data. This decouples design work from data modeling. | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | *None for MVP* | — | The default Blazor Server template includes everything needed | — | | `Microsoft.Playwright` | 1.41+ | Automated screenshot capture | Optional (Phase 2) | | `bunit` | 1.28+ | Component unit testing | Optional | | `Verify.Blazor` | 2.5+ | HTML snapshot testing | Optional | **The MVP requires zero additional NuGet packages.** Everything ships in the box with .NET 8 Blazor Server.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.razor components) | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches original design exactly | | **CSS Approach** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scoped styles per component, no build tooling needed | | **SVG Rendering** | Inline SVG via Razor markup | N/A | No library needed; use `<svg>`, `<line>`, `<polygon>`, `<circle>` | | **Fonts** | Segoe UI (system font) | N/A | Already on Windows; no web font loading | | **Icons** | None required | — | Legend uses inline CSS shapes (rotated squares, circles, lines) | **Do NOT use:** MudBlazor, Radzen, Syncfusion, or any component library. The design is simple enough that third-party UI frameworks add dependency weight and styling conflicts with zero benefit. The original HTML/CSS is ~150 lines; keep it lean. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Runtime** | .NET 8.0 LTS | 8.0.x (latest patch) | Long-term support through Nov 2026 | | **Web Host** | Kestrel (default) | Built-in | No IIS, no reverse proxy needed for local use | | **JSON Deserialization** | `System.Text.Json` | Built-in (.NET 8) | Fastest, zero-dependency JSON parsing | | **Configuration** | `data.json` loaded as a typed model | — | Use `JsonSerializer.Deserialize<T>()` | | **File Watching (optional)** | `FileSystemWatcher` | Built-in | Auto-reload dashboard when `data.json` changes | | Layer | Technology | Notes | |-------|-----------|-------| | **Storage** | `data.json` flat file | Single JSON file in project root or `wwwroot/data/` | | **Schema** | Strongly-typed C# POCOs | `DashboardData`, `Milestone`, `HeatmapRow`, `HeatmapItem` | | **No database** | — | Explicitly out of scope; JSON file is the entire data layer | | Layer | Technology | Notes | |-------|-----------|-------| | **Solution** | `.sln` with single `.csproj` | `ReportingDashboard.sln` → `ReportingDashboard.csproj` | | **Project Template** | `blazorserver` | `dotnet new blazorserver -n ReportingDashboard` | | **SDK** | `Microsoft.NET.Sdk.Web` | Standard web SDK | | Tool | Version | Purpose | |------|---------|---------| | **bUnit** | 1.28+ | Blazor component unit testing | | **xUnit** | 2.7+ | Test runner | | **Verify.Blazor** | 2.5+ | Snapshot testing for rendered HTML output | Testing is optional for a dashboard this simple but useful if iterating on layout fidelity. | Tool | Purpose | |------|---------| | `dotnet watch` | Hot reload during development | | Browser DevTools | Inspect CSS Grid, verify 1920×1080 layout | | **Playwright** (optional) | Automated screenshot capture at exact resolution | ---
```
App.razor
└── Dashboard.razor                  ← Single-page layout
    ├── DashboardHeader.razor        ← Title, subtitle, legend icons
    ├── TimelineSection.razor        ← SVG Gantt/milestone timeline
    │   ├── TimelineTrack.razor      ← One horizontal track (M1, M2, M3)
    │   └── MilestoneMarker.razor    ← Diamond (PoC/Prod), Circle (checkpoint)
    └── HeatmapSection.razor         ← Status grid
        ├── HeatmapHeader.razor      ← Column headers (Jan–Apr) + corner cell
        └── HeatmapRow.razor         ← One status row (Shipped/InProgress/etc.)
            └── HeatmapCell.razor    ← Individual cell with item bullets
``` **Total components: ~8.** This is intentionally flat. Do not over-engineer the component tree.
```
data.json
  → JsonSerializer.Deserialize<DashboardData>()
    → Registered as Singleton service (or Scoped)
      → Injected into Dashboard.razor via @inject
        → Passed as [Parameter] to child components
``` **No state management library needed.** No Fluxor, no cascading values (beyond what's trivial). The data flows one direction: file → service → components.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
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
  "heatmap": {
    "shipped": {
      "label": "Shipped",
      "colorClass": "ship",
      "items": {
        "Jan": ["Chatbot v1 launched", "KB ingestion pipeline"],
        "Feb": ["MS Role assignment"],
        "Mar": ["PDS connector beta"],
        "Apr": ["Auto-review DFD v1"]
      }
    },
    "inProgress": { ... },
    "carryover": { ... },
    "blockers": { ... }
  }
}
```
- **Use Blazor CSS Isolation** (`Dashboard.razor.css`, `HeatmapRow.razor.css`, etc.)
- Copy the color palette directly from the original HTML design:
- Shipped: `#1B7A28` header, `#E8F5E9` bg, `#F0FBF0` cells, `#D8F2DA` current month, `#34A853` bullets
- In Progress: `#1565C0` header, `#E3F2FD` bg, `#EEF4FE` cells, `#DAE8FB` current month, `#0078D4` bullets
- Carryover: `#B45309` header, `#FFF8E1` bg, `#FFFDE7` cells, `#FFF0B0` current month, `#F4B400` bullets
- Blockers: `#991B1B` header, `#FEF2F2` bg, `#FFF5F5` cells, `#FFE4E4` current month, `#EA4335` bullets
- **Fixed viewport approach:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` — this is a screenshot-first design, not a responsive web app.
- Grid layout: `grid-template-columns: 160px repeat(4, 1fr)` for heatmap, exactly as original. The timeline uses absolute positioning within an SVG viewBox. Recommended approach:
- Calculate pixel position from date: `x = (date - timelineStart) / (timelineEnd - timelineStart) * svgWidth`
- Render month gridlines as `<line>` elements
- Render milestone tracks as colored horizontal `<line>` elements
- Render markers:
- **Checkpoint** = `<circle>` (small, gray fill or white with colored stroke)
- **PoC Milestone** = `<polygon>` diamond, `#F4B400` fill, drop shadow filter
- **Production Release** = `<polygon>` diamond, `#34A853` fill, drop shadow filter
- **"NOW" line** = `<line>` dashed, `#EA4335`, with "NOW" text label
- All SVG is emitted inline in Razor—no JavaScript, no canvas, no charting library. ---

### Considerations & Risks

- **None.** This is explicitly out of scope per requirements. The app binds to `localhost:5000` (HTTP) only. No HTTPS certificate, no login, no cookies, no tokens. In `Program.cs`, configure:
```csharp
builder.WebHost.UseUrls("http://localhost:5000");
```
- `data.json` is a local file with no sensitive data (project names, dates, status labels).
- No PII, no credentials, no encryption needed.
- If the file contains sensitive project names, ensure the machine's filesystem permissions are adequate (standard user-level access). | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` from command line or VS debug | | **Port** | `http://localhost:5000` | | **Process** | Single Kestrel process, no reverse proxy | | **Deployment** | `dotnet publish -c Release -o ./publish` then `dotnet ReportingDashboard.dll` | | **No containers** | Docker is unnecessary for a local single-user tool | | **No CI/CD** | Manual build/run; optionally a `run.bat` / `run.ps1` script |
- **Startup time:** <2 seconds for Blazor Server cold start.
- **Memory:** <50MB RSS for this workload.
- **Screenshot workflow:** Open `http://localhost:5000` → browser full-screen at 1920×1080 → screenshot → paste into PowerPoint. Optionally automate with Playwright. --- | Risk | Severity | Mitigation | |------|----------|------------| | Blazor Server requires SignalR circuit | Low | Works fine locally; circuit disconnect is irrelevant for single-user screenshot workflow | | JSON schema drift | Low | Strongly-typed C# model will throw clear deserialization errors on schema mismatch | | SVG rendering differences across browsers | Low | Target one browser (Edge/Chrome); test once | | Risk | Severity | Mitigation | |------|----------|------------| | CSS Grid pixel-perfect fidelity | Medium | Use the original HTML as the ground truth; compare screenshots side-by-side | | Font rendering (Segoe UI availability) | Medium | Segoe UI ships with Windows; if targeting macOS for screenshots, add fallback fonts | | Data editing friction | Medium | Editing raw JSON is fine for a single maintainer; if multiple people update data, consider adding a minimal edit form later |
- **Blazor Server vs. Static HTML:** Blazor adds a runtime dependency (.NET 8 SDK or published binaries) compared to a plain HTML file. Trade-off accepted because: (a) Blazor enables clean data binding from `data.json`, (b) future enhancements (edit form, multiple projects) are trivial to add, (c) the team's stack is .NET.
- **No charting library:** Hand-crafted SVG means full control over pixel placement but requires manual coordinate math. This is the right call—charting libraries (Chart.js via JS interop, or Blazor wrappers like ApexCharts.Blazor) would add dependency weight and fight the custom design rather than help it.
- **No responsive design:** The page is fixed at 1920×1080. This is intentional for screenshot fidelity and simplifies CSS significantly. ---
- **How many milestone tracks?** The original design shows 3 (M1, M2, M3). Should `data.json` support an arbitrary number, or is 3 fixed? **Recommendation:** Support N tracks; the SVG height adjusts automatically.
- **How many months in the heatmap?** The design shows 4 columns (Jan–Apr). Should this be configurable (e.g., rolling 4 months, or a full fiscal year)? **Recommendation:** Make it data-driven from `data.json`; the grid adapts to however many months are provided.
- **"Current month" highlighting:** The design highlights the April column with a deeper background. Should the app auto-detect the current month, or should it be specified in `data.json`? **Recommendation:** Specify in `data.json` (`"currentMonth": "Apr"`) for screenshot consistency—you may want to regenerate a "March" view later.
- **Multiple projects/dashboards:** Will there ever be multiple `data.json` files for different projects? **Recommendation:** Start with one file. If needed later, add a route parameter: `/dashboard/{projectName}` that loads `data/{projectName}.json`.
- **Automated screenshot capture:** Should the build process include automated screenshot generation (Playwright headless at 1920×1080)? **Recommendation:** Nice-to-have for Phase 2; manual browser screenshot is fine for MVP. ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. It reads project milestone and status data from a `data.json` file and renders a visually polished view designed for screenshot capture at 1920×1080 for PowerPoint decks.

The design features three visual zones: a **header with legend**, an **SVG timeline/Gantt with milestone diamonds and checkpoints**, and a **color-coded heatmap grid** (Shipped/In Progress/Carryover/Blockers × months). The entire stack is intentionally simple—no auth, no database server, no enterprise middleware. The primary recommendation is a minimal Blazor Server app with inline SVG rendering, CSS Grid/Flexbox layout, and `System.Text.Json` for config deserialization. Total project complexity: low. Time to MVP: 1–2 days for a skilled .NET developer.

---

## 2. Key Findings

- The original HTML design is fully static with inline CSS—Blazor components can replicate this 1:1 with `.razor` files and scoped CSS isolation.
- SVG timeline rendering does **not** require a charting library; the original design uses hand-crafted `<svg>` elements with `<line>`, `<circle>`, `<polygon>`, and `<text>` which Blazor can emit directly via Razor markup.
- The heatmap grid uses CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`) and does not need a third-party grid component.
- Reading `data.json` at startup via `System.Text.Json` is sufficient; no database, no ORM, no caching layer needed.
- Blazor Server's SignalR circuit is irrelevant for this use case (single local user, no interactivity beyond page load), but it works fine and adds zero complexity.
- The design targets a fixed 1920×1080 viewport—responsive design is explicitly **not** needed; the page should be optimized for screenshot fidelity at that resolution.
- No authentication, authorization, or HTTPS configuration is needed. `dotnet run` on localhost is the deployment model.
- Hot reload (`dotnet watch`) provides excellent developer experience for iterating on layout.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.razor components) | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches original design exactly |
| **CSS Approach** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scoped styles per component, no build tooling needed |
| **SVG Rendering** | Inline SVG via Razor markup | N/A | No library needed; use `<svg>`, `<line>`, `<polygon>`, `<circle>` |
| **Fonts** | Segoe UI (system font) | N/A | Already on Windows; no web font loading |
| **Icons** | None required | — | Legend uses inline CSS shapes (rotated squares, circles, lines) |

**Do NOT use:** MudBlazor, Radzen, Syncfusion, or any component library. The design is simple enough that third-party UI frameworks add dependency weight and styling conflicts with zero benefit. The original HTML/CSS is ~150 lines; keep it lean.

### Backend (Minimal)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Runtime** | .NET 8.0 LTS | 8.0.x (latest patch) | Long-term support through Nov 2026 |
| **Web Host** | Kestrel (default) | Built-in | No IIS, no reverse proxy needed for local use |
| **JSON Deserialization** | `System.Text.Json` | Built-in (.NET 8) | Fastest, zero-dependency JSON parsing |
| **Configuration** | `data.json` loaded as a typed model | — | Use `JsonSerializer.Deserialize<T>()` |
| **File Watching (optional)** | `FileSystemWatcher` | Built-in | Auto-reload dashboard when `data.json` changes |

### Data Layer

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Storage** | `data.json` flat file | Single JSON file in project root or `wwwroot/data/` |
| **Schema** | Strongly-typed C# POCOs | `DashboardData`, `Milestone`, `HeatmapRow`, `HeatmapItem` |
| **No database** | — | Explicitly out of scope; JSON file is the entire data layer |

### Project Structure

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Solution** | `.sln` with single `.csproj` | `ReportingDashboard.sln` → `ReportingDashboard.csproj` |
| **Project Template** | `blazorserver` | `dotnet new blazorserver -n ReportingDashboard` |
| **SDK** | `Microsoft.NET.Sdk.Web` | Standard web SDK |

### Testing (Lightweight)

| Tool | Version | Purpose |
|------|---------|---------|
| **bUnit** | 1.28+ | Blazor component unit testing |
| **xUnit** | 2.7+ | Test runner |
| **Verify.Blazor** | 2.5+ | Snapshot testing for rendered HTML output |

Testing is optional for a dashboard this simple but useful if iterating on layout fidelity.

### Dev Tooling

| Tool | Purpose |
|------|---------|
| `dotnet watch` | Hot reload during development |
| Browser DevTools | Inspect CSS Grid, verify 1920×1080 layout |
| **Playwright** (optional) | Automated screenshot capture at exact resolution |

---

## 4. Architecture Recommendations

### Component Architecture (Maps to Visual Design Sections)

```
App.razor
└── Dashboard.razor                  ← Single-page layout
    ├── DashboardHeader.razor        ← Title, subtitle, legend icons
    ├── TimelineSection.razor        ← SVG Gantt/milestone timeline
    │   ├── TimelineTrack.razor      ← One horizontal track (M1, M2, M3)
    │   └── MilestoneMarker.razor    ← Diamond (PoC/Prod), Circle (checkpoint)
    └── HeatmapSection.razor         ← Status grid
        ├── HeatmapHeader.razor      ← Column headers (Jan–Apr) + corner cell
        └── HeatmapRow.razor         ← One status row (Shipped/InProgress/etc.)
            └── HeatmapCell.razor    ← Individual cell with item bullets
```

**Total components: ~8.** This is intentionally flat. Do not over-engineer the component tree.

### Data Flow Pattern

```
data.json
  → JsonSerializer.Deserialize<DashboardData>()
    → Registered as Singleton service (or Scoped)
      → Injected into Dashboard.razor via @inject
        → Passed as [Parameter] to child components
```

**No state management library needed.** No Fluxor, no cascading values (beyond what's trivial). The data flows one direction: file → service → components.

### Data Model (Recommended Schema for `data.json`)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
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
  "heatmap": {
    "shipped": {
      "label": "Shipped",
      "colorClass": "ship",
      "items": {
        "Jan": ["Chatbot v1 launched", "KB ingestion pipeline"],
        "Feb": ["MS Role assignment"],
        "Mar": ["PDS connector beta"],
        "Apr": ["Auto-review DFD v1"]
      }
    },
    "inProgress": { ... },
    "carryover": { ... },
    "blockers": { ... }
  }
}
```

### CSS Architecture

- **Use Blazor CSS Isolation** (`Dashboard.razor.css`, `HeatmapRow.razor.css`, etc.)
- Copy the color palette directly from the original HTML design:
  - Shipped: `#1B7A28` header, `#E8F5E9` bg, `#F0FBF0` cells, `#D8F2DA` current month, `#34A853` bullets
  - In Progress: `#1565C0` header, `#E3F2FD` bg, `#EEF4FE` cells, `#DAE8FB` current month, `#0078D4` bullets
  - Carryover: `#B45309` header, `#FFF8E1` bg, `#FFFDE7` cells, `#FFF0B0` current month, `#F4B400` bullets
  - Blockers: `#991B1B` header, `#FEF2F2` bg, `#FFF5F5` cells, `#FFE4E4` current month, `#EA4335` bullets
- **Fixed viewport approach:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` — this is a screenshot-first design, not a responsive web app.
- Grid layout: `grid-template-columns: 160px repeat(4, 1fr)` for heatmap, exactly as original.

### SVG Timeline Rendering

The timeline uses absolute positioning within an SVG viewBox. Recommended approach:

1. Calculate pixel position from date: `x = (date - timelineStart) / (timelineEnd - timelineStart) * svgWidth`
2. Render month gridlines as `<line>` elements
3. Render milestone tracks as colored horizontal `<line>` elements
4. Render markers:
   - **Checkpoint** = `<circle>` (small, gray fill or white with colored stroke)
   - **PoC Milestone** = `<polygon>` diamond, `#F4B400` fill, drop shadow filter
   - **Production Release** = `<polygon>` diamond, `#34A853` fill, drop shadow filter
   - **"NOW" line** = `<line>` dashed, `#EA4335`, with "NOW" text label
5. All SVG is emitted inline in Razor—no JavaScript, no canvas, no charting library.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope per requirements. The app binds to `localhost:5000` (HTTP) only. No HTTPS certificate, no login, no cookies, no tokens.

In `Program.cs`, configure:
```csharp
builder.WebHost.UseUrls("http://localhost:5000");
```

### Data Protection

- `data.json` is a local file with no sensitive data (project names, dates, status labels).
- No PII, no credentials, no encryption needed.
- If the file contains sensitive project names, ensure the machine's filesystem permissions are adequate (standard user-level access).

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` from command line or VS debug |
| **Port** | `http://localhost:5000` |
| **Process** | Single Kestrel process, no reverse proxy |
| **Deployment** | `dotnet publish -c Release -o ./publish` then `dotnet ReportingDashboard.dll` |
| **No containers** | Docker is unnecessary for a local single-user tool |
| **No CI/CD** | Manual build/run; optionally a `run.bat` / `run.ps1` script |

### Operational Concerns

- **Startup time:** <2 seconds for Blazor Server cold start.
- **Memory:** <50MB RSS for this workload.
- **Screenshot workflow:** Open `http://localhost:5000` → browser full-screen at 1920×1080 → screenshot → paste into PowerPoint. Optionally automate with Playwright.

---

## 6. Risks & Trade-offs

### Low Risks (Mitigated by Design)

| Risk | Severity | Mitigation |
|------|----------|------------|
| Blazor Server requires SignalR circuit | Low | Works fine locally; circuit disconnect is irrelevant for single-user screenshot workflow |
| JSON schema drift | Low | Strongly-typed C# model will throw clear deserialization errors on schema mismatch |
| SVG rendering differences across browsers | Low | Target one browser (Edge/Chrome); test once |

### Medium Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| CSS Grid pixel-perfect fidelity | Medium | Use the original HTML as the ground truth; compare screenshots side-by-side |
| Font rendering (Segoe UI availability) | Medium | Segoe UI ships with Windows; if targeting macOS for screenshots, add fallback fonts |
| Data editing friction | Medium | Editing raw JSON is fine for a single maintainer; if multiple people update data, consider adding a minimal edit form later |

### Trade-offs Accepted

1. **Blazor Server vs. Static HTML:** Blazor adds a runtime dependency (.NET 8 SDK or published binaries) compared to a plain HTML file. Trade-off accepted because: (a) Blazor enables clean data binding from `data.json`, (b) future enhancements (edit form, multiple projects) are trivial to add, (c) the team's stack is .NET.

2. **No charting library:** Hand-crafted SVG means full control over pixel placement but requires manual coordinate math. This is the right call—charting libraries (Chart.js via JS interop, or Blazor wrappers like ApexCharts.Blazor) would add dependency weight and fight the custom design rather than help it.

3. **No responsive design:** The page is fixed at 1920×1080. This is intentional for screenshot fidelity and simplifies CSS significantly.

---

## 7. Open Questions

1. **How many milestone tracks?** The original design shows 3 (M1, M2, M3). Should `data.json` support an arbitrary number, or is 3 fixed? **Recommendation:** Support N tracks; the SVG height adjusts automatically.

2. **How many months in the heatmap?** The design shows 4 columns (Jan–Apr). Should this be configurable (e.g., rolling 4 months, or a full fiscal year)? **Recommendation:** Make it data-driven from `data.json`; the grid adapts to however many months are provided.

3. **"Current month" highlighting:** The design highlights the April column with a deeper background. Should the app auto-detect the current month, or should it be specified in `data.json`? **Recommendation:** Specify in `data.json` (`"currentMonth": "Apr"`) for screenshot consistency—you may want to regenerate a "March" view later.

4. **Multiple projects/dashboards:** Will there ever be multiple `data.json` files for different projects? **Recommendation:** Start with one file. If needed later, add a route parameter: `/dashboard/{projectName}` that loads `data/{projectName}.json`.

5. **Automated screenshot capture:** Should the build process include automated screenshot generation (Playwright headless at 1920×1080)? **Recommendation:** Nice-to-have for Phase 2; manual browser screenshot is fine for MVP.

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-accurate reproduction of the original HTML design, driven by `data.json`.

1. **Scaffold project** (30 min)
   - `dotnet new blazorserver -n ReportingDashboard`
   - Remove default template pages (Counter, Weather, etc.)
   - Create `data.json` with fictional project data

2. **Build data model** (30 min)
   - Create `Models/DashboardData.cs` with POCOs matching JSON schema
   - Register a `DashboardDataService` singleton that deserializes `data.json` on startup
   - Add `FileSystemWatcher` for live reload when JSON changes

3. **Build header component** (30 min)
   - `Components/DashboardHeader.razor` — title, subtitle, legend
   - Copy CSS directly from original HTML, adapt to Blazor CSS isolation

4. **Build timeline/SVG component** (2–3 hours)
   - `Components/TimelineSection.razor` — track labels sidebar + SVG box
   - `Components/TimelineTrack.razor` — horizontal line with markers
   - Date-to-pixel coordinate calculation helper method
   - Diamond markers (`<polygon>`), circle markers (`<circle>`), NOW line

5. **Build heatmap component** (2–3 hours)
   - `Components/HeatmapSection.razor` — CSS Grid container
   - `Components/HeatmapRow.razor` — row header + data cells
   - Color class mapping from category (shipped/inProgress/carryover/blockers)
   - Current-month column highlighting

6. **Screenshot validation** (30 min)
   - Open at 1920×1080, compare with original design
   - Adjust spacing, font sizes, colors until match is satisfactory

### Phase 2: Polish (Optional, 1 day)

- Add Playwright screenshot automation (`dotnet test` generates PNG)
- Add a simple edit form (Blazor form that writes back to `data.json`)
- Add print-friendly CSS (`@media print`)
- Add a second "compact" layout variant for email embedding

### Quick Wins

1. **`dotnet watch` hot reload** — iterate on CSS and layout in real-time with near-instant feedback.
2. **Copy-paste the original CSS** — the HTML design's `<style>` block translates almost 1:1 into `.razor.css` files. Don't rewrite it; adapt it.
3. **Fictional project data first** — get the visuals right with fake data before wiring up real project data. This decouples design work from data modeling.

### Recommended NuGet Packages

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| *None for MVP* | — | The default Blazor Server template includes everything needed | — |
| `Microsoft.Playwright` | 1.41+ | Automated screenshot capture | Optional (Phase 2) |
| `bunit` | 1.28+ | Component unit testing | Optional |
| `Verify.Blazor` | 2.5+ | HTML snapshot testing | Optional |

**The MVP requires zero additional NuGet packages.** Everything ships in the box with .NET 8 Blazor Server.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/a483b197781b065f0e0a5bfb9ec7921bea3ca3ac/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
