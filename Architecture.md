# System Architecture: Design system architecture

## System Components

- **Blazor Server App**: Single-page dashboard rendering milestone timeline, shipped/in-progress/carried-over work items, and project health summary for executive reporting.
- **JSON Data Provider**: Reads `data.json` at runtime to populate all dashboard sections without a database.
- **Static Assets**: CSS styling inspired by `OriginalDesignConcept.html`, optimized for clean screenshot capture into PowerPoint decks.

## Data Model

- **Project**: Name, status, reporting period, overall health indicator, executive summary text.
- **Milestone**: Title, target date, completion date, status (completed/in-progress/upcoming) — displayed on the horizontal timeline.
- **WorkItem**: Title, description, category (shipped/in-progress/carried-over), owner, priority, and optional notes — grouped into card sections.

## Project Structure

```
ReportingDashboard.sln
ReportingDashboard/
├── ReportingDashboard.csproj
├── Program.cs
├── Components/
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   │   └── MainLayout.razor
│   └── Pages/
│       └── Dashboard.razor
├── Models/
│   ├── ProjectData.cs
│   ├── Milestone.cs
│   └── WorkItem.cs
├── Services/
│   └── DashboardDataService.cs
├── wwwroot/
│   ├── css/
│   │   └── dashboard.css
│   └── data.json
├── Properties/
│   └── launchSettings.json
└── appsettings.json
```

## Technology Choices

- **C# .NET 8 Blazor Server**: Interactive server-side rendering with no WASM download; ideal for local-only single-page dashboards.
- **System.Text.Json**: Deserializes `data.json` into strongly-typed models via `DashboardDataService`, registered as a singleton.
- **Pure CSS**: No JavaScript frameworks; lightweight styling ensures the page renders cleanly for screenshots, matching the simplicity of the original HTML design template.
- **No authentication or cloud services**: Runs on `localhost` only via `dotnet run`.