# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-18 06:57 UTC_

### Summary

Build a single-page Blazor Server app (.NET 8) that reads `data.json` at startup and renders a pixel-faithful recreation of the `OriginalDesignConcept.html` design: SVG timeline/Gantt at top, heatmap grid below (Shipped / In Progress / Carryover / Blockers). No auth, no cloud, no database — just a local `kestrel` server + JSON file. The app exists solely to produce clean 1920×1080 screenshots for PowerPoint decks. ---

### Key Findings

- The original design is pure HTML/CSS/SVG — Blazor Server can replicate this exactly via Razor components + inline SVG
- `System.Text.Json` (built-in .NET 8) is sufficient for deserializing `data.json` — no third-party ORM needed
- No charting library required; the timeline is hand-drawn SVG, which Blazor handles natively in `.razor` files
- CSS Grid (`grid-template-columns: 160px repeat(N, 1fr)`) is the correct layout for the heatmap — implement via scoped CSS
- Blazor Server SignalR overhead is irrelevant for a local, single-user, screenshot tool
- MudBlazor or Radzen would add unnecessary weight — avoid; raw Razor + CSS is better for pixel-perfect fidelity
- Hot reload (`dotnet watch`) enables fast design iteration without rebuilding ---
- Create `.sln` + Blazor Server project
- Define `data.json` schema + sample fictional project data
- Build `DashboardDataService` with JSON deserialization
- Implement `Dashboard.razor` with static layout matching `OriginalDesignConcept.html`
- Wire data into heatmap grid — hardcode 4 months initially
- Implement SVG timeline with dynamic milestone positioning
- Add "current month" column highlight driven by `DateTime.Now`
- Match exact color palette from design (`#34A853`, `#0078D4`, `#F4B400`, `#EA4335`) **Quick Win**: Get the heatmap grid rendering with fake data first — this is the most visually impactful section and validates the CSS Grid layout immediately.

### Recommended Tools & Technologies

- Blazor Server (.NET 8, `net8.0`) — Razor components for all UI
- Inline SVG in `.razor` files for the timeline/Gantt (no JS charting lib)
- Scoped CSS (`Component.razor.css`) for heatmap grid styles
- Segoe UI font (system font on Windows — no web font needed)
- ASP.NET Core 8 (Kestrel, local only — no IIS, no cloud)
- `System.Text.Json` 8.x (built-in) for `data.json` deserialization
- Singleton `DashboardDataService` injected via DI to cache parsed JSON
- None — `data.json` is the sole data source; loaded once at startup
- `dotnet run` / `dotnet watch` for local execution
- Single `.sln` with one project: `ReportingDashboard.csproj`
- Target URL: `http://localhost:5000` for browser screenshot
- bUnit 1.x — Blazor component unit tests
- xUnit 2.9.x — test host
- No E2E testing needed for a single-page screenshot tool ---
- **Single Razor page** (`/Pages/Dashboard.razor`) — no routing needed
- **Data model**: POCOs matching `data.json` schema (`ProjectConfig`, `Milestone`, `HeatmapRow`, `HeatmapCell`)
- **DashboardDataService** (singleton): reads + deserializes `data.json` on first access, caches in memory
- **SVG Timeline**: computed from milestone dates in `data.json`; X positions calculated as `(date - startDate).TotalDays / totalDays * svgWidth`
- **Heatmap**: `foreach` over `HeatmapRow[]` → `foreach` over months → render cell items
- Layout: fixed `width: 1920px; height: 1080px; overflow: hidden` to match screenshot dimensions ---

### Considerations & Risks

- No auth — local tool, no sensitive data
- No HTTPS required locally (`http://localhost:5000`)
- `data.json` stored in project root or `wwwroot/data/` — loaded via `IWebHostEnvironment.ContentRootPath`
- No external network calls; fully air-gapped capable
- Deploy: `dotnet publish -c Release` → xcopy to target machine --- | Risk | Mitigation | |---|---| | SVG X-position math breaks with variable date ranges | Encapsulate in a `TimelineCalculator` helper; unit test it | | `data.json` schema changes break deserialization | Use `JsonSerializerOptions.PropertyNameCaseInsensitive = true`; add validation on load | | 1920px fixed width clips on smaller monitors | Acceptable — tool is for screenshots, not responsive viewing | | Blazor Server SignalR keeps connection open | No issue for local single-user; use `<base href="/">` correctly | ---
- Should `data.json` be hot-reloaded (file watcher) or only at app startup?
- How many months should the heatmap support — fixed 4 (Jan–Apr) or dynamic?
- Should the "current month" highlight column be auto-detected from system date or hardcoded in `data.json`?
- Will multiple project configs be needed (multi-project switcher) or always single project? ---

### Detailed Analysis

# Research.md — Executive Reporting Dashboard (My Project)

## Executive Summary

Build a single-page Blazor Server app (.NET 8) that reads `data.json` at startup and renders a pixel-faithful recreation of the `OriginalDesignConcept.html` design: SVG timeline/Gantt at top, heatmap grid below (Shipped / In Progress / Carryover / Blockers). No auth, no cloud, no database — just a local `kestrel` server + JSON file. The app exists solely to produce clean 1920×1080 screenshots for PowerPoint decks.

---

## Key Findings

- The original design is pure HTML/CSS/SVG — Blazor Server can replicate this exactly via Razor components + inline SVG
- `System.Text.Json` (built-in .NET 8) is sufficient for deserializing `data.json` — no third-party ORM needed
- No charting library required; the timeline is hand-drawn SVG, which Blazor handles natively in `.razor` files
- CSS Grid (`grid-template-columns: 160px repeat(N, 1fr)`) is the correct layout for the heatmap — implement via scoped CSS
- Blazor Server SignalR overhead is irrelevant for a local, single-user, screenshot tool
- MudBlazor or Radzen would add unnecessary weight — avoid; raw Razor + CSS is better for pixel-perfect fidelity
- Hot reload (`dotnet watch`) enables fast design iteration without rebuilding

---

## Recommended Technology Stack

**Frontend**
- Blazor Server (.NET 8, `net8.0`) — Razor components for all UI
- Inline SVG in `.razor` files for the timeline/Gantt (no JS charting lib)
- Scoped CSS (`Component.razor.css`) for heatmap grid styles
- Segoe UI font (system font on Windows — no web font needed)

**Backend**
- ASP.NET Core 8 (Kestrel, local only — no IIS, no cloud)
- `System.Text.Json` 8.x (built-in) for `data.json` deserialization
- Singleton `DashboardDataService` injected via DI to cache parsed JSON

**Database**
- None — `data.json` is the sole data source; loaded once at startup

**Infrastructure**
- `dotnet run` / `dotnet watch` for local execution
- Single `.sln` with one project: `ReportingDashboard.csproj`
- Target URL: `http://localhost:5000` for browser screenshot

**Testing**
- bUnit 1.x — Blazor component unit tests
- xUnit 2.9.x — test host
- No E2E testing needed for a single-page screenshot tool

---

## Architecture Recommendations

- **Single Razor page** (`/Pages/Dashboard.razor`) — no routing needed
- **Data model**: POCOs matching `data.json` schema (`ProjectConfig`, `Milestone`, `HeatmapRow`, `HeatmapCell`)
- **DashboardDataService** (singleton): reads + deserializes `data.json` on first access, caches in memory
- **SVG Timeline**: computed from milestone dates in `data.json`; X positions calculated as `(date - startDate).TotalDays / totalDays * svgWidth`
- **Heatmap**: `foreach` over `HeatmapRow[]` → `foreach` over months → render cell items
- Layout: fixed `width: 1920px; height: 1080px; overflow: hidden` to match screenshot dimensions

---

## Security & Infrastructure

- No auth — local tool, no sensitive data
- No HTTPS required locally (`http://localhost:5000`)
- `data.json` stored in project root or `wwwroot/data/` — loaded via `IWebHostEnvironment.ContentRootPath`
- No external network calls; fully air-gapped capable
- Deploy: `dotnet publish -c Release` → xcopy to target machine

---

## Risks & Trade-offs

| Risk | Mitigation |
|---|---|
| SVG X-position math breaks with variable date ranges | Encapsulate in a `TimelineCalculator` helper; unit test it |
| `data.json` schema changes break deserialization | Use `JsonSerializerOptions.PropertyNameCaseInsensitive = true`; add validation on load |
| 1920px fixed width clips on smaller monitors | Acceptable — tool is for screenshots, not responsive viewing |
| Blazor Server SignalR keeps connection open | No issue for local single-user; use `<base href="/">` correctly |

---

## Open Questions

- Should `data.json` be hot-reloaded (file watcher) or only at app startup?
- How many months should the heatmap support — fixed 4 (Jan–Apr) or dynamic?
- Should the "current month" highlight column be auto-detected from system date or hardcoded in `data.json`?
- Will multiple project configs be needed (multi-project switcher) or always single project?

---

## Implementation Recommendations

**Phase 1 — MVP (2–3 days)**
1. Create `.sln` + Blazor Server project
2. Define `data.json` schema + sample fictional project data
3. Build `DashboardDataService` with JSON deserialization
4. Implement `Dashboard.razor` with static layout matching `OriginalDesignConcept.html`
5. Wire data into heatmap grid — hardcode 4 months initially

**Phase 2 — Polish (1 day)**
1. Implement SVG timeline with dynamic milestone positioning
2. Add "current month" column highlight driven by `DateTime.Now`
3. Match exact color palette from design (`#34A853`, `#0078D4`, `#F4B400`, `#EA4335`)

**Quick Win**: Get the heatmap grid rendering with fake data first — this is the most visually impactful section and validates the CSS Grid layout immediately.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/4600a6948e53997216c0d22f757194f43d7ce089/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
