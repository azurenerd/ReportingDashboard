# Reporting Dashboard

A single-page executive reporting dashboard that visualizes project milestones, shipped deliverables, in-progress work, carryover items, and blockers. Optimized for 1920×1080 PowerPoint screenshot capture.

## Quick Start

```bash
cd src/ReportingDashboard
dotnet restore
dotnet run
```

Navigate to `http://localhost:5000` (or `https://localhost:5001`).

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (LTS)
- Windows 10/11 recommended (Segoe UI font)

## Updating Dashboard Data

All display data is driven by a single JSON configuration file:

```
src/ReportingDashboard/Data/dashboard-data.json
```

Edit this file to update:
- Project title, subtitle, and backlog URL
- Timeline date range and milestone streams
- Heatmap months and work item entries
- Current date/month for highlighting

After editing, restart the application (`Ctrl+C` then `dotnet run`) to see changes.

### JSON Schema Validation

The configuration file references `dashboard-data.schema.json` for IDE auto-complete and validation. In VS Code, the `$schema` property enables IntelliSense and real-time validation.

## Taking Screenshots

1. Open Edge/Chrome at `http://localhost:5000`
2. Set browser window to 1920×1080 (use DevTools device toolbar or resize)
3. Use `Win+Shift+S` or `Print Screen` to capture
4. Paste directly into a 16:9 PowerPoint slide

## Architecture

- **Framework:** Blazor Server (.NET 8)
- **Data:** Single JSON file (no database, no APIs)
- **Rendering:** Server-side HTML via SignalR, inline SVG for timeline
- **CSS:** Fixed 1920×1080 viewport, CSS Grid heatmap, Flexbox layout
- **Dependencies:** Zero third-party NuGet packages

## Project Structure

```
src/ReportingDashboard/
├── Program.cs                    Application host
├── Data/
│   ├── dashboard-data.json       Configuration (edit this!)
│   └── dashboard-data.schema.json  JSON Schema for validation
├── Models/
│   ├── DashboardData.cs          Root data model
│   ├── MilestoneStream.cs        Timeline stream
│   ├── Milestone.cs              Individual milestone marker
│   └── HeatmapRow.cs             Heatmap category row
├── Services/
│   └── DashboardDataService.cs   JSON loading & caching
├── Components/
│   ├── App.razor                 Root component
│   ├── Routes.razor              Router
│   ├── _Imports.razor            Global usings
│   ├── Layout/
│   │   └── MainLayout.razor      Minimal HTML shell
│   ├── Pages/
│   │   └── Dashboard.razor       Main page (route: "/")
│   ├── DashboardHeader.razor     Header section
│   ├── TimelineSection.razor     SVG timeline
│   └── HeatmapGrid.razor         CSS Grid heatmap
└── wwwroot/
    └── css/
        └── dashboard.css         All styles
```

## Configuration Reference

| Field | Type | Description |
|-------|------|-------------|
| `title` | string | Project title (header, 24px bold) |
| `subtitle` | string | Organization path and period |
| `backlogUrl` | string (URL) | ADO Backlog hyperlink |
| `currentDate` | date (YYYY-MM-DD) | NOW indicator position |
| `timelineStartDate` | date | Timeline left edge |
| `timelineEndDate` | date | Timeline right edge |
| `milestoneStreams` | array (1-5) | Timeline tracks with milestones |
| `heatmapMonths` | array (3-6) | Column headers |
| `currentMonth` | string | Highlighted month column |
| `heatmapRows` | array (exactly 4) | Category rows with items |

### Milestone Types

| Type | Visual | Description |
|------|--------|-------------|
| `checkpoint` | White circle with colored stroke | Standard checkpoint |
| `checkpoint-small` | Small gray dot | Minor checkpoint (no label) |
| `poc` | Amber diamond (#F4B400) | Proof of Concept milestone |
| `production` | Green diamond (#34A853) | Production release |

## Error Handling

- **Missing JSON file:** Console displays clear message with expected path
- **Invalid JSON syntax:** Console shows parse error with position details
- **Missing fields:** Graceful degradation (empty/partial render)

## Browser Support

- **Primary:** Microsoft Edge (Chromium) on Windows 10/11
- **Secondary:** Chrome, Firefox (minor SVG text differences possible)

## License

Internal use only.
