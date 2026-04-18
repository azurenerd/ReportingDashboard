# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-18 03:08 UTC_

### Summary

Build a single-page Blazor Server app (.NET 8) that reads `data.json` at startup, renders a 1920×1080-optimized executive dashboard matching the `OriginalDesignConcept.html` design: SVG milestone timeline + color-coded heatmap grid. No auth, no cloud, no database — just file I/O + in-memory state. This is the fastest path to screenshot-ready PowerPoint slides. ---

### Key Findings

- The design is static-layout-first (fixed 1920px width), making Blazor Server ideal — no SPA complexity needed
- SVG timeline must be rendered inline in Razor; no charting library needed (design uses hand-crafted SVG)
- `data.json` replaces any database; `System.Text.Json` deserializes it natively in .NET 8
- The heatmap uses CSS Grid (`160px repeat(N,1fr)`) — pure CSS, no component library required
- Blazor Server's SignalR overhead is irrelevant for a single local user viewing a static report
- Segoe UI font is pre-installed on Windows — no web font CDN needed (local-only confirmed) ---
- Create `.sln`, Blazor Server project, define `data.json` using the schema above + C# models
- Wire `DashboardDataService` to read and deserialize `data.json`
- Render heatmap grid with hardcoded fake data to validate CSS layout
- Implement SVG timeline with milestone diamonds, circles, "NOW" line from data
- Apply all CSS classes from design spec (ship/prog/carry/block color schemes)
- Populate from `data.json` fake fictional project data
- Tune 1920×1080 pixel-perfect layout for screenshot quality
- Validate against `OriginalDesignConcept.html` visually side-by-side
- Add bUnit smoke tests for data binding **Quick Win**: Get the heatmap grid rendering with CSS in hour one — it's the largest visual surface and confirms the layout approach works before investing in SVG math.

### Recommended Tools & Technologies

- Blazor Server — .NET 8 (`Microsoft.AspNetCore.Components` 8.x, included in SDK)
- Inline SVG in `.razor` components for timeline rendering
- Scoped CSS (`.razor.css`) for heatmap grid — no CSS framework needed
- Font: `font-family: 'Segoe UI', Arial, sans-serif` (system font, zero dependencies)
- ASP.NET Core 8 (Kestrel, local only, `http://localhost:5000`)
- `System.Text.Json` 8.x for `data.json` deserialization (built-in)
- `IOptionsMonitor<T>` or manual `File.ReadAllText` + `JsonSerializer.Deserialize<T>` for hot-reload of data file
- `data.json` flat file — no SQLite, no EF Core
- Typed C# model classes: `DashboardConfig`, `Milestone`, `HeatmapRow`, `HeatmapCell`
- See concrete schema definition in Architecture Recommendations section
- `.sln` with two projects: `ReportingDashboard` (Blazor Server app) + `ReportingDashboard.Tests` (xUnit)
- Run via `dotnet run` locally; no Docker, no IIS required
- **Recommended**: xUnit 2.9.x + bUnit 1.x for Blazor component unit testing
- *Pros*: Native Blazor component rendering, fast, no browser required, well-maintained
- *Con*: Cannot validate final rendered pixel output
- **Alternative**: Microsoft Playwright 1.43.x (`Microsoft.Playwright` NuGet) for E2E screenshot validation
- *Pros*: Can automate browser screenshot capture for regression comparison; validates actual 1920×1080 render
- *Con*: Requires Chromium install, significantly more setup overhead, overkill for a read-only static dashboard
- *Verdict*: Defer Playwright unless pixel-regression testing becomes a requirement; bUnit is sufficient for data-binding correctness ---
- **Single-page architecture**: One `Dashboard.razor` component, no routing needed
- **Data flow**: `Program.cs` registers `DashboardDataService` as singleton → reads `data.json` once on startup → injects into page component
- **SVG timeline**: Computed from milestone data in C# (x-position = `(date - startDate).TotalDays / totalDays * svgWidth`) rendered as inline Razor SVG elements
- **Heatmap**: `foreach` over rows/columns in Razor, CSS classes driven by row type enum (`Shipped`, `InProgress`, `Carryover`, `Blocker`)
- **No state management needed** — read-only dashboard
```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "adoLink": "https://dev.azure.com/org/project/_backlogs"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-17",
    "milestones": [
      {
        "id": "m1",
        "label": "M1",
        "description": "Chatbot & MS Role",
        "color": "#0078D4",
        "events": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "release", "label": "May Prod" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
    "currentColumn": "Apr",
    "rows": [
      {
        "type": "Shipped",
        "label": "Shipped",
        "cells": [
          { "month": "Jan", "items": ["Feature A deployed", "Bug fix #1234"] },
          { "month": "Apr", "items": ["Feature B GA release"] }
        ]
      },
      {
        "type": "InProgress",
        "label": "In Progress",
        "cells": [
          { "month": "Apr", "items": ["Feature C — 80% done", "Integration tests"] }
        ]
      },
      {
        "type": "Carryover",
        "label": "Carryover",
        "cells": [
          { "month": "Apr", "items": ["Feature D (from Mar)"] }
        ]
      },
      {
        "type": "Blocker",
        "label": "Blockers",
        "cells": [
          { "month": "Apr", "items": ["Dependency on Platform team API"] }
        ]
      }
    ]
  }
}
``` ---

### Considerations & Risks

- **Auth**: None (by design — local screenshot tool)
- **Hosting**: `dotnet run` on localhost; bind to `127.0.0.1` only in `appsettings.json`
- **Data**: `data.json` is plaintext config — no PII, no encryption needed
- **Cost**: $0 — runs on developer's workstation
- **Logging/Observability**: Use built-in `ILogger<T>` (ASP.NET Core 8, no extra packages) configured to write to console and optionally a rolling file via `appsettings.json`. Log startup errors (e.g., missing/malformed `data.json`) at `Error` level. No telemetry, no APM tooling needed. Sample config:
  ```json
  "Logging": {
    "LogLevel": { "Default": "Warning", "ReportingDashboard": "Information" },
    "Console": { "IncludeScopes": false }
  }
  ``` For file logging without extra packages, redirect console output: `dotnet run > dashboard.log 2>&1` ---
- **SVG x-position math**: Date-to-pixel mapping must account for variable month lengths — use `DateOnly` diff, not month index arithmetic
- **1920px fixed layout**: Will look broken on smaller screens — acceptable since screenshots are the output, not responsive use
- **Blazor Server SignalR**: Adds minor latency on first load vs. static HTML — mitigate by keeping component tree shallow
- **data.json schema drift**: Strongly type the model and validate on startup; throw descriptive errors if required fields are missing ---
- Should `data.json` be watched for changes (auto-refresh on file save) or require app restart?
- How many months should the timeline span? (Design shows Jan–Jun; is this configurable per project?)
- Should the "current month" column highlight be computed from system clock or hardcoded in `data.json`?
- Will multiple projects need separate data files, or is one dashboard per run sufficient? ---

### Detailed Analysis

# Research.md — Executive Reporting Dashboard (Blazor Server)

## Executive Summary
Build a single-page Blazor Server app (.NET 8) that reads `data.json` at startup, renders a 1920×1080-optimized executive dashboard matching the `OriginalDesignConcept.html` design: SVG milestone timeline + color-coded heatmap grid. No auth, no cloud, no database — just file I/O + in-memory state. This is the fastest path to screenshot-ready PowerPoint slides.

---

## Key Findings
- The design is static-layout-first (fixed 1920px width), making Blazor Server ideal — no SPA complexity needed
- SVG timeline must be rendered inline in Razor; no charting library needed (design uses hand-crafted SVG)
- `data.json` replaces any database; `System.Text.Json` deserializes it natively in .NET 8
- The heatmap uses CSS Grid (`160px repeat(N,1fr)`) — pure CSS, no component library required
- Blazor Server's SignalR overhead is irrelevant for a single local user viewing a static report
- Segoe UI font is pre-installed on Windows — no web font CDN needed (local-only confirmed)

---

## Recommended Technology Stack

**Frontend (Razor/CSS)**
- Blazor Server — .NET 8 (`Microsoft.AspNetCore.Components` 8.x, included in SDK)
- Inline SVG in `.razor` components for timeline rendering
- Scoped CSS (`.razor.css`) for heatmap grid — no CSS framework needed
- Font: `font-family: 'Segoe UI', Arial, sans-serif` (system font, zero dependencies)

**Backend**
- ASP.NET Core 8 (Kestrel, local only, `http://localhost:5000`)
- `System.Text.Json` 8.x for `data.json` deserialization (built-in)
- `IOptionsMonitor<T>` or manual `File.ReadAllText` + `JsonSerializer.Deserialize<T>` for hot-reload of data file

**Data**
- `data.json` flat file — no SQLite, no EF Core
- Typed C# model classes: `DashboardConfig`, `Milestone`, `HeatmapRow`, `HeatmapCell`

**Infrastructure**
- `.sln` with two projects: `ReportingDashboard` (Blazor Server app) + `ReportingDashboard.Tests` (xUnit)
- Run via `dotnet run` locally; no Docker, no IIS required

**Testing**
- xUnit 2.9.x + `bUnit` 1.x for Blazor component testing
- No integration test infrastructure needed (local file read is trivial)

---

## Architecture Recommendations
- **Single-page architecture**: One `Dashboard.razor` component, no routing needed
- **Data flow**: `Program.cs` registers `DashboardDataService` as singleton → reads `data.json` once on startup → injects into page component
- **SVG timeline**: Computed from milestone data in C# (x-position = `(date - startDate).TotalDays / totalDays * svgWidth`) rendered as `@((MarkupString)svgHtml)` or inline Razor SVG elements
- **Heatmap**: `foreach` over rows/columns in Razor, CSS classes driven by row type enum (`Shipped`, `InProgress`, `Carryover`, `Blocker`)
- **No state management needed** — read-only dashboard

---

## Security & Infrastructure
- **Auth**: None (by design — local screenshot tool)
- **Hosting**: `dotnet run` on localhost; bind to `127.0.0.1` only in `appsettings.json`
- **Data**: `data.json` is plaintext config — no PII, no encryption needed
- **Cost**: $0 — runs on developer's workstation

---

## Risks & Trade-offs
- **SVG x-position math**: Date-to-pixel mapping must account for variable month lengths — use `DateOnly` diff, not month index arithmetic
- **1920px fixed layout**: Will look broken on smaller screens — acceptable since screenshots are the output, not responsive use
- **Blazor Server SignalR**: Adds minor latency on first load vs. static HTML — mitigate by keeping component tree shallow
- **data.json schema drift**: Strongly type the model and validate on startup; throw descriptive errors if required fields are missing

---

## Open Questions
- Should `data.json` be watched for changes (auto-refresh on file save) or require app restart?
- How many months should the timeline span? (Design shows Jan–Jun; is this configurable per project?)
- Should the "current month" column highlight be computed from system clock or hardcoded in `data.json`?
- Will multiple projects need separate data files, or is one dashboard per run sufficient?

---

## Implementation Recommendations

**Phase 1 — Skeleton (Day 1)**
- Create `.sln`, Blazor Server project, define `data.json` schema + C# models
- Wire `DashboardDataService` to read and deserialize `data.json`
- Render heatmap grid with hardcoded fake data to validate CSS layout

**Phase 2 — Full Design (Day 2)**
- Implement SVG timeline with milestone diamonds, circles, "NOW" line from data
- Apply all CSS classes from design spec (ship/prog/carry/block color schemes)
- Populate from `data.json` fake fictional project data

**Phase 3 — Polish (Day 3)**
- Tune 1920×1080 pixel-perfect layout for screenshot quality
- Validate against `OriginalDesignConcept.html` visually side-by-side
- Add bUnit smoke tests for data binding

**Quick Win**: Get the heatmap grid rendering with CSS in hour one — it's the largest visual surface and confirms the layout approach works before investing in SVG math.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/a459dea0e9e2dab3f589f3b5965639deed4d7417/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
