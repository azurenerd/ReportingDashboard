# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-07 20:46 UTC_

### Summary

Building a simple, single-page executive reporting dashboard using C# .NET 8 Blazor Server with local-only deployment. The solution will read project milestone and progress data from a JSON configuration file, render an enhanced version of the provided HTML design template, and support screenshot/export capabilities for PowerPoint integration. This is a lightweight application prioritizing simplicity, clarity, and rapid visualization—no authentication, cloud services, or enterprise complexity required. ---

### Key Findings

- Blazor Server eliminates the need for separate frontend technology while maintaining full .NET ecosystem access
- JSON-based configuration eliminates database complexity for a single-page reporting tool
- Chart.js and Plotly.NET provide lightweight, screenshot-friendly visualizations without heavy dependencies
- Local-only deployment simplifies infrastructure to a single Windows application or IIS hosting
- Razor components enable template reuse and component-driven UI architecture
- File-based data storage (JSON) sufficient for non-distributed, single-instance reporting
- HTML design template can be directly converted to Blazor Razor components with minimal refactoring
- Screenshot-optimized CSS media queries essential for PowerPoint export quality
- ---
- Blazor Server project scaffold (.NET 8)
- Dashboard.razor layout matching OriginalDesignConcept.html
- data.json schema + sample data (3-5 fictional projects)
- ProjectDataService (file read, JSON deserialization)
- MilestoneTimeline.razor (HTML table or Chart.js timeline)
- ProgressCard.razor (status counts, completion %)
- Basic CSS for print-friendly layout
- Manual refresh button (no file watching)
- Day 1: Recreate HTML design in Razor components; verify layout matches
- Day 2: Add Chart.js timeline; populate with sample data
- Day 3: Implement data.json loading; test with real project data
- Week 1: Screenshot optimization; validate PowerPoint export quality
- **Before committing**: Test Chart.js rendering quality in screenshot form (Snagit/Windows Snippet Tool)
- **Design iteration**: Get executive feedback on layout before finalizing components
- **Data schema**: Validate JSON structure with stakeholders; finalize before service implementation
- File system watcher for auto-refresh
- Docker containerization for non-Windows deployments
- Windows Authentication for corporate network integration
- Burndown chart visualization
- Export to PowerPoint XML (via Open XML SDK)
- Historical snapshots/trends page
- ---
- Review OriginalDesignConcept.html file structure
- Create data.json schema document
- Bootstrap Blazor Server project
- Begin Component-Razor translation

### Recommended Tools & Technologies

- **Blazor Server** (C# .NET 8, .NET Core 8.0)
- Strengths: Full C# in browser via WebSocket, component reusability, server-side rendering
- Maturity: Production-ready; Microsoft-backed; widely adopted
- Community: Large .NET ecosystem; Blazor-specific libraries growing rapidly
- **Razor Components** (built-in)
- For dashboard layout, milestone timeline, progress indicators
- Version: Included with .NET 8
- **Chart.js 4.x** (via CDN or NuGet package `ChartJs.Blazor`)
- Lightweight, screenshot-friendly charts
- Alternative: Plotly.NET for advanced timeline visualization
- **Bootstrap 5.3** (CSS framework)
- Quick responsive layout; minimal custom styling required
- Included via CDN or NuGet `Bootstrap` package
- **ASP.NET Core 8.0** (built-in with .NET 8)
- RESTful API endpoints for dashboard data
- Static file serving for JSON configuration
- **System.Text.Json** (built-in)
- Native JSON deserialization for data.json
- No external dependency; high performance
- **File-based JSON** (data.json)
- Single source of truth for project milestones, status, progress
- Schema: `{ projects: [{ name, milestones: [{ name, date, status }], status: [{ category, count }] }] }`
- No database required; manual updates or file watcher for refresh
- **FileSystemWatcher** (.NET built-in)
- Optional: Monitor data.json for changes; auto-refresh dashboard
- **Visual Studio 2022 Community** (or Code + Omnisharp extension)
- Full Blazor debugging support
- Built-in .NET 8 toolchain
- **.NET CLI 8.0**
- Command-line project/package management
- Local development server: `dotnet watch run`
- **xUnit 2.x** (unit testing)
- Lightweight; .NET standard
- **Moq** (mocking library)
- Simple dependency mocking for data services
- **HtmlRenderer.Core** (optional, for server-side HTML rendering)
- Alternative: Playwright/Selenium for automated screenshots
- For MVP: Manual screenshot via browser; optimize CSS for print media
- ---
- ```
- Dashboard.Web/
- ├── Components/
- │   ├── Dashboard.razor          (main container)
- │   ├── MilestoneTimeline.razor  (Gantt/timeline visualization)
- │   ├── ProgressCard.razor       (status indicators)
- │   └── ProjectSummary.razor     (high-level overview)
- ├── Services/
- │   ├── ProjectDataService.cs    (loads/deserializes data.json)
- │   └── RefreshService.cs        (polls file changes if needed)
- ├── Models/
- │   ├── Project.cs
- │   ├── Milestone.cs
- │   └── StatusItem.cs
- ├── wwwroot/
- │   ├── data.json                (configuration file)
- │   ├── css/                     (custom styles for dashboard)
- │   └── design-template.html     (reference from OriginalDesignConcept)
- └── Program.cs                   (Blazor Server setup)
- ```
- Dashboard.razor loads on page request
- ProjectDataService reads data.json from wwwroot at component initialization
- Razor components deserialize JSON into typed models
- Chart.js renders timeline; HTML/CSS renders status cards
- (Optional) FileSystemWatcher monitors data.json; Blazor SignalR triggers client refresh
- **Reusable Razor components** for milestone cards, progress bars, status badges
- **Data binding** via @bind directives for real-time updates
- **CSS Grid/Flexbox** for responsive, screenshot-friendly layout
- **Print-friendly media queries** to optimize for PowerPoint screenshots
- Server-side Razor rendering for static HTML structure
- Client-side interactivity via Blazor WebSocket (minimal)
- Chart.js for client-rendered timeline/charts
- No client-side framework overhead; all logic in C#
- ---

### Considerations & Risks

- **None required** for MVP (internal tool, local deployment)
- Future: Add Windows Authentication if deployed to corporate network
- No sensitive data; project metadata only
- data.json stored in wwwroot; no encryption needed
- Optional: Restrict file permissions to application identity user
- **Local IIS** (Windows Server) or **Windows Service** hosting
- Alternative: Docker container for portability (optional)
- No cloud services per requirements
- **Single-machine deployment**: IIS application pool with .NET 8 runtime
- **Startup**: Automatic app start on Windows boot via IIS application pool recycling
- **Port**: HTTP port 80 or custom port (e.g., 8080)
- **Zero cloud cost** (local only)
- **Hardware**: Minimal (1 GB RAM, modest CPU sufficient)
- **Monitoring**: Windows Event Viewer for errors; optional Application Insights (local setup)
- ---
- | Risk | Severity | Mitigation |
- |------|----------|-----------|
- | Blazor Server WebSocket disconnects on network glitch | Medium | Implement reconnection logic; validate data on reconnect |
- | data.json corruption from concurrent writes | Medium | File locking; implement atomic write (temp file + rename) |
- | Large JSON files slow initial load | Low | Paginate milestones; implement lazy-loading for older projects |
- | Chart.js rendering delays for 100+ milestones | Low | Aggregate milestones by quarter; client-side virtualization |
- Single Blazor Server instance handles ~100 concurrent users (browser connections)
- JSON file I/O is I/O-bound; use FileSystemWatcher sparingly
- No built-in caching; add in-memory cache if data.json accessed frequently
- **Simplicity vs. Flexibility**: File-based JSON easier than database, but limits querying/filtering
- **Server-rendered vs. SPA**: Blazor Server easier to build, but slightly higher server load than Blazor WebAssembly
- **Local-only vs. Cloud**: No scalability; acceptable for internal tool
- Blazor Server is relatively new; ensure team has C# experience
- Chart.js integration straightforward; limited learning curve
- ---
- **Data Update Frequency**: Manual (admin edits JSON) or automated (CI/CD pipeline writes JSON)?
- **Multi-project Dashboard**: Display single project or multiple projects on same page?
- **Historical Data**: Archive previous months' snapshots for trend analysis?
- **Export Format**: PNG/PDF screenshots only, or structured export (Excel, JSON)?
- **Refresh Behavior**: Auto-refresh on data.json change, or manual "Refresh" button?
- **Browser Support**: Modern browsers only, or IE11 compatibility required?
- ---

### Detailed Analysis

# Research.md: Executive Reporting Dashboard Technology Stack

## Executive Summary

Building a simple, single-page executive reporting dashboard using C# .NET 8 Blazor Server with local-only deployment. The solution will read project milestone and progress data from a JSON configuration file, render an enhanced version of the provided HTML design template, and support screenshot/export capabilities for PowerPoint integration. This is a lightweight application prioritizing simplicity, clarity, and rapid visualization—no authentication, cloud services, or enterprise complexity required.

**Primary Recommendation**: Blazor Server (C# .NET 8) with Razor components, client-side JSON deserialization, and Chart.js/Plotly.NET for timeline and progress visualization. Store data in a local JSON file with file-system polling or manual refresh.

---

## Key Findings

- Blazor Server eliminates the need for separate frontend technology while maintaining full .NET ecosystem access
- JSON-based configuration eliminates database complexity for a single-page reporting tool
- Chart.js and Plotly.NET provide lightweight, screenshot-friendly visualizations without heavy dependencies
- Local-only deployment simplifies infrastructure to a single Windows application or IIS hosting
- Razor components enable template reuse and component-driven UI architecture
- File-based data storage (JSON) sufficient for non-distributed, single-instance reporting
- HTML design template can be directly converted to Blazor Razor components with minimal refactoring
- Screenshot-optimized CSS media queries essential for PowerPoint export quality

---

## Recommended Technology Stack

### Frontend Layer
- **Blazor Server** (C# .NET 8, .NET Core 8.0)
  - Strengths: Full C# in browser via WebSocket, component reusability, server-side rendering
  - Maturity: Production-ready; Microsoft-backed; widely adopted
  - Community: Large .NET ecosystem; Blazor-specific libraries growing rapidly
  
- **Razor Components** (built-in)
  - For dashboard layout, milestone timeline, progress indicators
  - Version: Included with .NET 8
  
- **Chart.js 4.x** (via CDN or NuGet package `ChartJs.Blazor`)
  - Lightweight, screenshot-friendly charts
  - Alternative: Plotly.NET for advanced timeline visualization
  
- **Bootstrap 5.3** (CSS framework)
  - Quick responsive layout; minimal custom styling required
  - Included via CDN or NuGet `Bootstrap` package

### Backend Layer
- **ASP.NET Core 8.0** (built-in with .NET 8)
  - RESTful API endpoints for dashboard data
  - Static file serving for JSON configuration
  
- **System.Text.Json** (built-in)
  - Native JSON deserialization for data.json
  - No external dependency; high performance

### Data Storage
- **File-based JSON** (data.json)
  - Single source of truth for project milestones, status, progress
  - Schema: `{ projects: [{ name, milestones: [{ name, date, status }], status: [{ category, count }] }] }`
  - No database required; manual updates or file watcher for refresh
  
- **FileSystemWatcher** (.NET built-in)
  - Optional: Monitor data.json for changes; auto-refresh dashboard

### Development Tools
- **Visual Studio 2022 Community** (or Code + Omnisharp extension)
  - Full Blazor debugging support
  - Built-in .NET 8 toolchain
  
- **.NET CLI 8.0**
  - Command-line project/package management
  - Local development server: `dotnet watch run`

### Testing & Quality
- **xUnit 2.x** (unit testing)
  - Lightweight; .NET standard
  
- **Moq** (mocking library)
  - Simple dependency mocking for data services

### Export/Screenshot Optimization
- **HtmlRenderer.Core** (optional, for server-side HTML rendering)
  - Alternative: Playwright/Selenium for automated screenshots
  - For MVP: Manual screenshot via browser; optimize CSS for print media

---

## Architecture Recommendations

### Application Structure
```
Dashboard.Web/
├── Components/
│   ├── Dashboard.razor          (main container)
│   ├── MilestoneTimeline.razor  (Gantt/timeline visualization)
│   ├── ProgressCard.razor       (status indicators)
│   └── ProjectSummary.razor     (high-level overview)
├── Services/
│   ├── ProjectDataService.cs    (loads/deserializes data.json)
│   └── RefreshService.cs        (polls file changes if needed)
├── Models/
│   ├── Project.cs
│   ├── Milestone.cs
│   └── StatusItem.cs
├── wwwroot/
│   ├── data.json                (configuration file)
│   ├── css/                     (custom styles for dashboard)
│   └── design-template.html     (reference from OriginalDesignConcept)
└── Program.cs                   (Blazor Server setup)
```

### Data Flow
1. Dashboard.razor loads on page request
2. ProjectDataService reads data.json from wwwroot at component initialization
3. Razor components deserialize JSON into typed models
4. Chart.js renders timeline; HTML/CSS renders status cards
5. (Optional) FileSystemWatcher monitors data.json; Blazor SignalR triggers client refresh

### Design Pattern: Component-Based UI
- **Reusable Razor components** for milestone cards, progress bars, status badges
- **Data binding** via @bind directives for real-time updates
- **CSS Grid/Flexbox** for responsive, screenshot-friendly layout
- **Print-friendly media queries** to optimize for PowerPoint screenshots

### Rendering Strategy
- Server-side Razor rendering for static HTML structure
- Client-side interactivity via Blazor WebSocket (minimal)
- Chart.js for client-rendered timeline/charts
- No client-side framework overhead; all logic in C#

---

## Security & Infrastructure

### Authentication & Authorization
- **None required** for MVP (internal tool, local deployment)
- Future: Add Windows Authentication if deployed to corporate network

### Data Protection
- No sensitive data; project metadata only
- data.json stored in wwwroot; no encryption needed
- Optional: Restrict file permissions to application identity user

### Hosting & Deployment
- **Local IIS** (Windows Server) or **Windows Service** hosting
  - Alternative: Docker container for portability (optional)
  - No cloud services per requirements
  
- **Single-machine deployment**: IIS application pool with .NET 8 runtime
- **Startup**: Automatic app start on Windows boot via IIS application pool recycling
- **Port**: HTTP port 80 or custom port (e.g., 8080)

### Infrastructure Cost
- **Zero cloud cost** (local only)
- **Hardware**: Minimal (1 GB RAM, modest CPU sufficient)
- **Monitoring**: Windows Event Viewer for errors; optional Application Insights (local setup)

---

## Risks & Trade-offs

### Technical Risks
| Risk | Severity | Mitigation |
|------|----------|-----------|
| Blazor Server WebSocket disconnects on network glitch | Medium | Implement reconnection logic; validate data on reconnect |
| data.json corruption from concurrent writes | Medium | File locking; implement atomic write (temp file + rename) |
| Large JSON files slow initial load | Low | Paginate milestones; implement lazy-loading for older projects |
| Chart.js rendering delays for 100+ milestones | Low | Aggregate milestones by quarter; client-side virtualization |

### Scalability Bottlenecks
- Single Blazor Server instance handles ~100 concurrent users (browser connections)
- JSON file I/O is I/O-bound; use FileSystemWatcher sparingly
- No built-in caching; add in-memory cache if data.json accessed frequently

### Trade-offs
- **Simplicity vs. Flexibility**: File-based JSON easier than database, but limits querying/filtering
- **Server-rendered vs. SPA**: Blazor Server easier to build, but slightly higher server load than Blazor WebAssembly
- **Local-only vs. Cloud**: No scalability; acceptable for internal tool

### Skills Gap
- Blazor Server is relatively new; ensure team has C# experience
- Chart.js integration straightforward; limited learning curve

---

## Open Questions

1. **Data Update Frequency**: Manual (admin edits JSON) or automated (CI/CD pipeline writes JSON)?
2. **Multi-project Dashboard**: Display single project or multiple projects on same page?
3. **Historical Data**: Archive previous months' snapshots for trend analysis?
4. **Export Format**: PNG/PDF screenshots only, or structured export (Excel, JSON)?
5. **Refresh Behavior**: Auto-refresh on data.json change, or manual "Refresh" button?
6. **Browser Support**: Modern browsers only, or IE11 compatibility required?

---

## Implementation Recommendations

### MVP Scope (Phase 1, 2-3 weeks)
1. Blazor Server project scaffold (.NET 8)
2. Dashboard.razor layout matching OriginalDesignConcept.html
3. data.json schema + sample data (3-5 fictional projects)
4. ProjectDataService (file read, JSON deserialization)
5. MilestoneTimeline.razor (HTML table or Chart.js timeline)
6. ProgressCard.razor (status counts, completion %)
7. Basic CSS for print-friendly layout
8. Manual refresh button (no file watching)

### Quick Wins (Immediate Value)
- Day 1: Recreate HTML design in Razor components; verify layout matches
- Day 2: Add Chart.js timeline; populate with sample data
- Day 3: Implement data.json loading; test with real project data
- Week 1: Screenshot optimization; validate PowerPoint export quality

### Prototyping Recommendations
- **Before committing**: Test Chart.js rendering quality in screenshot form (Snagit/Windows Snippet Tool)
- **Design iteration**: Get executive feedback on layout before finalizing components
- **Data schema**: Validate JSON structure with stakeholders; finalize before service implementation

### Future Enhancements (Post-MVP)
- File system watcher for auto-refresh
- Docker containerization for non-Windows deployments
- Windows Authentication for corporate network integration
- Burndown chart visualization
- Export to PowerPoint XML (via Open XML SDK)
- Historical snapshots/trends page

---

**Next Steps**: 
1. Review OriginalDesignConcept.html file structure
2. Create data.json schema document
3. Bootstrap Blazor Server project
4. Begin Component-Razor translation
