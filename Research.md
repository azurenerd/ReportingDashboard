# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 02:51 UTC_

### Summary

This executive reporting dashboard should be implemented as a **Blazor Server application with an embedded JSON data configuration system**. The design is inherently simple and screenshot-friendly—leverage this by building a stateless, single-page component that renders directly to HTML/CSS without complex interactivity. Use **SignalR for real-time updates** if data changes during viewing, **NodaTime for date handling** to match the timeline, and **built-in .NET JSON serialization** to load the data.json configuration. No authentication, no databases, no cloud dependencies—just a clean, responsive Blazor component that renders the four-row heatmap and timeline SVG. This approach gets you to executive-ready screenshots within 1-2 weeks and is maintainable enough that a single developer can own the codebase.

### Key Findings

- **Blazor Server is ideal for this use case**: No JavaScript framework complexity, full C# in the browser, native HTML/CSS rendering for pixel-perfect screenshot quality.
- **The HTML design template is production-ready**: Don't rebuild the CSS or layout logic—extract and reuse the exact grid structure, color palette, and fonts (Segoe UI) directly.
- **JSON as a data source is sufficient**: No database needed. A simple data.json file with project name, status rows (shipped/in-progress/carryover/blockers), timeline milestones, and month columns covers all executive reporting needs.
- **Rendering is deterministic and repeatable**: SVG timeline and CSS Grid heatmap produce identical output every time, making automated screenshot generation feasible.
- **Performance is not a constraint**: A single dashboard rendering once per session has zero scalability concerns. Bundle size and load time are negligible at Blazor Server scale.
- **Maintenance burden is minimal**: No auth, no caching strategy, no distributed state, no external service dependencies—only C# component logic + CSS + JSON file updates.
- **Week 1: Foundation**
- Create Blazor Server project (.NET 8 LTS)
- Copy OriginalDesignConcept.html CSS into wwwroot/css/dashboard.css
- Design and document data.json schema (see sample below)
- Create DashboardConfig POCOs + system.Text.Json deserialization
- Create dummy data.json with 1 fictional project, 4 quarters, 4 status rows, 3 milestones
- **Week 1.5: Rendering**
- Build Dashboard.razor component (scaffold from OriginalDesignConcept.html structure)
- Render heatmap grid with hardcoded sample data
- Render timeline SVG with milestone markers
- Apply exact CSS styling from design template
- Test in browser; capture screenshot for visual regression
- **Week 2: Polish**
- Wire up DashboardDataService to load data.json
- Parameterize colors (use VisualizationService for color mapping)
- Add date calculation logic (NodaTime for month boundaries)
- Write unit tests for JSON deserialization + date math
- Automated screenshot test (Playwright: load page, assert PNG matches baseline)
- Documentation: data.json schema, deployment steps, screenshot instructions
- **"Last Updated" timestamp**: File.GetLastWriteTime(data.json) + display in header (5 min)
- **Dark mode toggle**: Add CSS media query + localStorage switch (15 min)
- **Favicon + custom title**: Brand the browser tab (5 min)
- **Print stylesheet**: Ensure heatmap prints on single page (10 min)
- **Before finalizing data schema**: Mock 3 different project types (platform, feature, operations) in dummy data to ensure schema flexibility
- **Before committing to SVG generation**: Test SVG coordinates with sample milestones; compare hand-coded SVG vs. programmatic generation for accuracy
- **Before deploying internally**: Share single screenshot with stakeholders for color/layout feedback; defer feature requests until post-MVP
```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "description": "Trusted Platform — Privacy Automation Workstream — April 2026",
  "quarters": [
    {
      "month": "April",
      "year": 2026,
      "shipped": [
        "Chatbot MVP",
        "Role Templates"
      ],
      "inProgress": [
        "Data Inventory",
        "Policy Engine"
      ],
      "carryover": [
        "DFD Automation"
      ],
      "blockers": [
        "Azure AD Sync Dependency"
      ]
    }
  ],
  "milestones": [
    {
      "id": "m1",
      "label": "Chatbot & MS Role",
      "date": "2026-01-12",
      "type": "checkpoint"
    },
    {
      "id": "m2-poc",
      "label": "PoC Milestone",
      "date": "2026-03-26",
      "type": "poc"
    },
    {
      "id": "m3-prod",
      "label": "Production Release",
      "date": "2026-04-30",
      "type": "release"
    }
  ]
}
```
- [ ] .NET 8 SDK installed on target machine
- [ ] data.json file copied to wwwroot/data/ directory
- [ ] appsettings.json configured for environment (local vs. internal network)
- [ ] Test from another machine on same network (verify Kestrel is listening on 0.0.0.0)
- [ ] Screenshot captured and confirmed for PowerPoint
- [ ] Documented: "To update dashboard, edit data.json and refresh browser"

### Recommended Tools & Technologies

- **Blazor Server** (.NET 8 LTS, Microsoft.AspNetCore.Components.Web 8.0.x)
- Full server-side rendering with WebSocket signaling (built-in)
- Reason: Matches C# skillset, produces clean HTML/CSS for screenshots, no Node.js build pipeline needed
- **Tailwind CSS 3.x or inline Segoe UI stylesheet**
- Reuse the exact CSS from OriginalDesignConcept.html; minimal CSS tooling needed
- **SVG.NET for timeline generation** (Sves.Lib 2.x, if programmatic SVG needed)
- OR: Render SVG directly from Blazor using string templates (simplest path)
- Coordinates are deterministic; no dynamic charting library required
- **C# .NET 8 LTS runtime**
- Reason: Long-term support, performance-optimized
- **System.Text.Json** (built-in, .NET 8)
- Deserialize data.json config; no third-party JSON library needed
- **NodaTime 4.1.x** (for robust date/month calculations in timeline)
- Reason: Handles calendar math (month boundaries, Feb leap years) cleanly; lighter than DateTime gymnastics
- Alternative: Built-in DateTime if project timeline is simple (< 12 months), but NodaTime scales better
- **data.json** (local file, committed to repo)
- Schema: `{ projectName, description, quarters: [{ month, shipped: [], inProgress: [], carryover: [], blockers: [] }], milestones: [{ id, label, date, type: "poc"|"release"|"checkpoint" }] }`
- No external APIs; single source of truth for all dashboard updates
- **IIS Express or Kestrel** (local development)
- Reason: Ships with .NET 8 SDK; no additional infrastructure
- **Optional: Docker container** (if local sharing across machines)
- Dockerfile: Multi-stage .NET 8 build, target 80/443, serve static assets + Blazor WASM fallback
- **No CDN, no cloud services**: Serve from localhost or internal network only
- **xUnit 2.x** (industry standard in .NET)
- Unit test JSON deserialization, date calculations, heatmap data aggregation logic
- **Playwright .NET or Selenium** (for screenshot regression testing)
- Automate: load dashboard, verify row colors, verify timeline milestone positions, capture PNG
- **StyleCop.Analyzers** (code style enforcement, .NET community default)
- **Visual Studio 2022 Community or JetBrains Rider** (IDE)
- **.NET 8 SDK** (dotnet CLI for builds, testing, publishing)
- **Git** (version control)
```
AgentSquad.ReportingDashboard/
├── Program.cs                          # Blazor Server setup
├── Pages/
│   └── Dashboard.razor                 # Single page component
├── Components/
│   ├── HeatmapGrid.razor               # Row/column grid (reusable)
│   ├── TimelineChart.razor             # SVG timeline (reusable)
│   └── Legend.razor                    # Color legend
├── Services/
│   ├── DashboardDataService.cs         # Load & parse data.json
│   ├── DateCalculationService.cs       # NodaTime helpers (month boundaries, milestones)
│   └── VisualizationService.cs         # SVG generation, heatmap color mapping
├── Models/
│   ├── DashboardConfig.cs              # Root config schema
│   ├── ProjectStatus.cs                # Shipped/InProgress/Carryover/Blockers rows
│   └── Milestone.cs                    # Timeline events
├── wwwroot/
│   ├── css/dashboard.css               # Exact CSS from OriginalDesignConcept.html
│   ├── data/data.json                  # Project data config
│   └── img/                            # Any logos or static images
└── Tests/
    ├── DashboardDataServiceTests.cs
    └── VisualizationServiceTests.cs
```
- **On app load**: DashboardDataService reads data.json via File.ReadAllText() + System.Text.Json.JsonSerializer.Deserialize()
- **Deserialization**: Maps JSON to DashboardConfig (POCO objects)
- **In Dashboard.razor**:
- Call DateCalculationService.CalculateMonthColumns() → determine 4-month grid layout
- Call VisualizationService.MapStatusToColor() → assign heatmap cell background colors
- Call VisualizationService.GenerateTimelineSVG() → render milestone timeline
- **Rendering**: Blazor server renders component to HTML, browser displays
- **No state needed**: Dashboard is read-only from user perspective
- **Optional: SignalR auto-refresh** if data.json changes during viewing:
- FileSystemWatcher monitors data.json
- Triggers HubConnection.InvokeAsync("RefreshDashboard")
- Component calls StateHasChanged() to re-render
- **Month columns**: Use NodaTime.LocalDate + YearMonth to align milestones to month grids
- **"Now" marker**: Fixed to current date; highlight current month column with different background
- **Milestone types**: "poc", "release", "checkpoint" → SVG polygon/circle shapes (use shapes from OriginalDesignConcept.html)

### Considerations & Risks

- **None**: Dashboard is read-only, accessible to anyone with network access to localhost or internal network
- **If future expansion needed**: Add simple bearer token validation (JWT via configuration, no database lookup)
- **data.json is not encrypted**: Assume internal network only; no sensitive data in heatmap (status strings, milestone names only)
- **Blazor Server wire protocol is unencrypted by default**: Use HTTPS in production if deployed beyond localhost (Kestrel + self-signed cert is sufficient)
- **Local development**: IIS Express, port 5000
- **Internal network sharing**: Kestrel listening on `http://0.0.0.0:5000` (all interfaces)
- **Firewall**: Restrict port 5000 to trusted subnets only (not internet-facing)
- **appsettings.json**: Single entry for data file path (can be overridden per environment)
  ```json
  {
    "DashboardDataPath": "./wwwroot/data/data.json",
    "AllowRemoteAccess": false
  }
  ```
- **Development**: dotnet run from Visual Studio or CLI
- **Internal share**: Publish as self-contained exe + copy to shared network folder
- Command: `dotnet publish -c Release -r win-x64 --self-contained`
- Copy output to shared drive, run .exe
- **No Docker needed** unless sharing across Windows/Linux; single .NET 8 runtime simplifies distribution | Risk | Mitigation | |------|-----------| | **Heatmap scales poorly beyond 12 months** | Limit display to 4–6 months; use pagination or rolling window if needed | | **data.json becomes stale** | Document that only the data.json file needs updates; no database sync required | | **Timeline SVG hard-coded coordinates** | Use NodaTime to calculate month positions programmatically; regenerate SVG on each render | | **Color palette inconsistency** | Extract colors to C# constants in VisualizationService; single source of truth for all theme colors | | **Screenshot reproducibility** | All rendering is deterministic (no animations, no randomness); automated screenshot testing is feasible | | **Single point of failure (data.json)** | Keep a backup; data.json is human-readable, easy to restore from version control |
- **Chosen: Simplicity** for MVP (no database, no auth, no caching)
- **Cost**: If executives want drill-down (click heatmap cell → show detailed task list), you'll need to add deeper data structure + Blazor event handlers
- **Mitigation**: Design data.json schema to be extensible (each heatmap cell can reference optional details array)
- **How frequently does data.json update?** (Daily? Weekly? Once per sprint?)
- Affects caching strategy and auto-refresh feature viability
- **Should the dashboard support multiple projects or only one at a time?**
- Single project: keep current design; multiple projects: add project selector dropdown
- **Are there specific color accessibility (WCAG) requirements?**
- Current palette uses distinct hues; test with colorblind simulation if needed
- **Should milestones be clickable (navigate to ADO/Jira)?**
- Current design is screenshot-ready (no hyperlinks); adding links requires URL config
- **Is a "last updated" timestamp needed?**
- Useful for executives to know data staleness; adds 1 line of metadata
- **Should the dashboard print cleanly to PDF?**
- Current CSS layout is print-friendly; test with browser print dialog

### Detailed Analysis

# Executive Dashboard Research

## Executive Summary

This executive reporting dashboard should be implemented as a **Blazor Server application with an embedded JSON data configuration system**. The design is inherently simple and screenshot-friendly—leverage this by building a stateless, single-page component that renders directly to HTML/CSS without complex interactivity. Use **SignalR for real-time updates** if data changes during viewing, **NodaTime for date handling** to match the timeline, and **built-in .NET JSON serialization** to load the data.json configuration. No authentication, no databases, no cloud dependencies—just a clean, responsive Blazor component that renders the four-row heatmap and timeline SVG. This approach gets you to executive-ready screenshots within 1-2 weeks and is maintainable enough that a single developer can own the codebase.

## Key Findings

- **Blazor Server is ideal for this use case**: No JavaScript framework complexity, full C# in the browser, native HTML/CSS rendering for pixel-perfect screenshot quality.
- **The HTML design template is production-ready**: Don't rebuild the CSS or layout logic—extract and reuse the exact grid structure, color palette, and fonts (Segoe UI) directly.
- **JSON as a data source is sufficient**: No database needed. A simple data.json file with project name, status rows (shipped/in-progress/carryover/blockers), timeline milestones, and month columns covers all executive reporting needs.
- **Rendering is deterministic and repeatable**: SVG timeline and CSS Grid heatmap produce identical output every time, making automated screenshot generation feasible.
- **Performance is not a constraint**: A single dashboard rendering once per session has zero scalability concerns. Bundle size and load time are negligible at Blazor Server scale.
- **Maintenance burden is minimal**: No auth, no caching strategy, no distributed state, no external service dependencies—only C# component logic + CSS + JSON file updates.

## Recommended Technology Stack

### Frontend
- **Blazor Server** (.NET 8 LTS, Microsoft.AspNetCore.Components.Web 8.0.x)
  - Full server-side rendering with WebSocket signaling (built-in)
  - Reason: Matches C# skillset, produces clean HTML/CSS for screenshots, no Node.js build pipeline needed
- **Tailwind CSS 3.x or inline Segoe UI stylesheet**
  - Reuse the exact CSS from OriginalDesignConcept.html; minimal CSS tooling needed
- **SVG.NET for timeline generation** (Sves.Lib 2.x, if programmatic SVG needed)
  - OR: Render SVG directly from Blazor using string templates (simplest path)
  - Coordinates are deterministic; no dynamic charting library required

### Backend
- **C# .NET 8 LTS runtime**
  - Reason: Long-term support, performance-optimized
- **System.Text.Json** (built-in, .NET 8)
  - Deserialize data.json config; no third-party JSON library needed
- **NodaTime 4.1.x** (for robust date/month calculations in timeline)
  - Reason: Handles calendar math (month boundaries, Feb leap years) cleanly; lighter than DateTime gymnastics
  - Alternative: Built-in DateTime if project timeline is simple (< 12 months), but NodaTime scales better

### Data Source
- **data.json** (local file, committed to repo)
  - Schema: `{ projectName, description, quarters: [{ month, shipped: [], inProgress: [], carryover: [], blockers: [] }], milestones: [{ id, label, date, type: "poc"|"release"|"checkpoint" }] }`
  - No external APIs; single source of truth for all dashboard updates

### Infrastructure & Deployment
- **IIS Express or Kestrel** (local development)
  - Reason: Ships with .NET 8 SDK; no additional infrastructure
- **Optional: Docker container** (if local sharing across machines)
  - Dockerfile: Multi-stage .NET 8 build, target 80/443, serve static assets + Blazor WASM fallback
- **No CDN, no cloud services**: Serve from localhost or internal network only

### Testing & Quality
- **xUnit 2.x** (industry standard in .NET)
  - Unit test JSON deserialization, date calculations, heatmap data aggregation logic
- **Playwright .NET or Selenium** (for screenshot regression testing)
  - Automate: load dashboard, verify row colors, verify timeline milestone positions, capture PNG
- **StyleCop.Analyzers** (code style enforcement, .NET community default)

### Development Tools
- **Visual Studio 2022 Community or JetBrains Rider** (IDE)
- **.NET 8 SDK** (dotnet CLI for builds, testing, publishing)
- **Git** (version control)

## Architecture Recommendations

### Application Structure
```
AgentSquad.ReportingDashboard/
├── Program.cs                          # Blazor Server setup
├── Pages/
│   └── Dashboard.razor                 # Single page component
├── Components/
│   ├── HeatmapGrid.razor               # Row/column grid (reusable)
│   ├── TimelineChart.razor             # SVG timeline (reusable)
│   └── Legend.razor                    # Color legend
├── Services/
│   ├── DashboardDataService.cs         # Load & parse data.json
│   ├── DateCalculationService.cs       # NodaTime helpers (month boundaries, milestones)
│   └── VisualizationService.cs         # SVG generation, heatmap color mapping
├── Models/
│   ├── DashboardConfig.cs              # Root config schema
│   ├── ProjectStatus.cs                # Shipped/InProgress/Carryover/Blockers rows
│   └── Milestone.cs                    # Timeline events
├── wwwroot/
│   ├── css/dashboard.css               # Exact CSS from OriginalDesignConcept.html
│   ├── data/data.json                  # Project data config
│   └── img/                            # Any logos or static images
└── Tests/
    ├── DashboardDataServiceTests.cs
    └── VisualizationServiceTests.cs
```

### Data Flow
1. **On app load**: DashboardDataService reads data.json via File.ReadAllText() + System.Text.Json.JsonSerializer.Deserialize()
2. **Deserialization**: Maps JSON to DashboardConfig (POCO objects)
3. **In Dashboard.razor**: 
   - Call DateCalculationService.CalculateMonthColumns() → determine 4-month grid layout
   - Call VisualizationService.MapStatusToColor() → assign heatmap cell background colors
   - Call VisualizationService.GenerateTimelineSVG() → render milestone timeline
4. **Rendering**: Blazor server renders component to HTML, browser displays

### State Management
- **No state needed**: Dashboard is read-only from user perspective
- **Optional: SignalR auto-refresh** if data.json changes during viewing:
  - FileSystemWatcher monitors data.json
  - Triggers HubConnection.InvokeAsync("RefreshDashboard")
  - Component calls StateHasChanged() to re-render

### Date/Timeline Logic
- **Month columns**: Use NodaTime.LocalDate + YearMonth to align milestones to month grids
- **"Now" marker**: Fixed to current date; highlight current month column with different background
- **Milestone types**: "poc", "release", "checkpoint" → SVG polygon/circle shapes (use shapes from OriginalDesignConcept.html)

## Security & Infrastructure

### Authentication & Authorization
- **None**: Dashboard is read-only, accessible to anyone with network access to localhost or internal network
- **If future expansion needed**: Add simple bearer token validation (JWT via configuration, no database lookup)

### Data Protection
- **data.json is not encrypted**: Assume internal network only; no sensitive data in heatmap (status strings, milestone names only)
- **Blazor Server wire protocol is unencrypted by default**: Use HTTPS in production if deployed beyond localhost (Kestrel + self-signed cert is sufficient)

### Network & Hosting
- **Local development**: IIS Express, port 5000
- **Internal network sharing**: Kestrel listening on `http://0.0.0.0:5000` (all interfaces)
- **Firewall**: Restrict port 5000 to trusted subnets only (not internet-facing)

### Configuration Management
- **appsettings.json**: Single entry for data file path (can be overridden per environment)
  ```json
  {
    "DashboardDataPath": "./wwwroot/data/data.json",
    "AllowRemoteAccess": false
  }
  ```

### Deployment
- **Development**: dotnet run from Visual Studio or CLI
- **Internal share**: Publish as self-contained exe + copy to shared network folder
  - Command: `dotnet publish -c Release -r win-x64 --self-contained`
  - Copy output to shared drive, run .exe
- **No Docker needed** unless sharing across Windows/Linux; single .NET 8 runtime simplifies distribution

## Risks & Trade-offs

| Risk | Mitigation |
|------|-----------|
| **Heatmap scales poorly beyond 12 months** | Limit display to 4–6 months; use pagination or rolling window if needed |
| **data.json becomes stale** | Document that only the data.json file needs updates; no database sync required |
| **Timeline SVG hard-coded coordinates** | Use NodaTime to calculate month positions programmatically; regenerate SVG on each render |
| **Color palette inconsistency** | Extract colors to C# constants in VisualizationService; single source of truth for all theme colors |
| **Screenshot reproducibility** | All rendering is deterministic (no animations, no randomness); automated screenshot testing is feasible |
| **Single point of failure (data.json)** | Keep a backup; data.json is human-readable, easy to restore from version control |

### Trade-off: Simplicity vs. Extensibility
- **Chosen: Simplicity** for MVP (no database, no auth, no caching)
- **Cost**: If executives want drill-down (click heatmap cell → show detailed task list), you'll need to add deeper data structure + Blazor event handlers
- **Mitigation**: Design data.json schema to be extensible (each heatmap cell can reference optional details array)

## Open Questions

1. **How frequently does data.json update?** (Daily? Weekly? Once per sprint?)
   - Affects caching strategy and auto-refresh feature viability
2. **Should the dashboard support multiple projects or only one at a time?**
   - Single project: keep current design; multiple projects: add project selector dropdown
3. **Are there specific color accessibility (WCAG) requirements?**
   - Current palette uses distinct hues; test with colorblind simulation if needed
4. **Should milestones be clickable (navigate to ADO/Jira)?**
   - Current design is screenshot-ready (no hyperlinks); adding links requires URL config
5. **Is a "last updated" timestamp needed?**
   - Useful for executives to know data staleness; adds 1 line of metadata
6. **Should the dashboard print cleanly to PDF?**
   - Current CSS layout is print-friendly; test with browser print dialog

## Implementation Recommendations

### MVP Scope (Week 1–2)
1. **Week 1: Foundation**
   - Create Blazor Server project (.NET 8 LTS)
   - Copy OriginalDesignConcept.html CSS into wwwroot/css/dashboard.css
   - Design and document data.json schema (see sample below)
   - Create DashboardConfig POCOs + system.Text.Json deserialization
   - Create dummy data.json with 1 fictional project, 4 quarters, 4 status rows, 3 milestones

2. **Week 1.5: Rendering**
   - Build Dashboard.razor component (scaffold from OriginalDesignConcept.html structure)
   - Render heatmap grid with hardcoded sample data
   - Render timeline SVG with milestone markers
   - Apply exact CSS styling from design template
   - Test in browser; capture screenshot for visual regression

3. **Week 2: Polish**
   - Wire up DashboardDataService to load data.json
   - Parameterize colors (use VisualizationService for color mapping)
   - Add date calculation logic (NodaTime for month boundaries)
   - Write unit tests for JSON deserialization + date math
   - Automated screenshot test (Playwright: load page, assert PNG matches baseline)
   - Documentation: data.json schema, deployment steps, screenshot instructions

### Quick Wins (High Value, Low Effort)
- **"Last Updated" timestamp**: File.GetLastWriteTime(data.json) + display in header (5 min)
- **Dark mode toggle**: Add CSS media query + localStorage switch (15 min)
- **Favicon + custom title**: Brand the browser tab (5 min)
- **Print stylesheet**: Ensure heatmap prints on single page (10 min)

### Prototyping Recommendations
1. **Before finalizing data schema**: Mock 3 different project types (platform, feature, operations) in dummy data to ensure schema flexibility
2. **Before committing to SVG generation**: Test SVG coordinates with sample milestones; compare hand-coded SVG vs. programmatic generation for accuracy
3. **Before deploying internally**: Share single screenshot with stakeholders for color/layout feedback; defer feature requests until post-MVP

### Sample data.json Schema
```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "description": "Trusted Platform — Privacy Automation Workstream — April 2026",
  "quarters": [
    {
      "month": "April",
      "year": 2026,
      "shipped": [
        "Chatbot MVP",
        "Role Templates"
      ],
      "inProgress": [
        "Data Inventory",
        "Policy Engine"
      ],
      "carryover": [
        "DFD Automation"
      ],
      "blockers": [
        "Azure AD Sync Dependency"
      ]
    }
  ],
  "milestones": [
    {
      "id": "m1",
      "label": "Chatbot & MS Role",
      "date": "2026-01-12",
      "type": "checkpoint"
    },
    {
      "id": "m2-poc",
      "label": "PoC Milestone",
      "date": "2026-03-26",
      "type": "poc"
    },
    {
      "id": "m3-prod",
      "label": "Production Release",
      "date": "2026-04-30",
      "type": "release"
    }
  ]
}
```

### Deployment Checklist
- [ ] .NET 8 SDK installed on target machine
- [ ] data.json file copied to wwwroot/data/ directory
- [ ] appsettings.json configured for environment (local vs. internal network)
- [ ] Test from another machine on same network (verify Kestrel is listening on 0.0.0.0)
- [ ] Screenshot captured and confirmed for PowerPoint
- [ ] Documented: "To update dashboard, edit data.json and refresh browser"

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
