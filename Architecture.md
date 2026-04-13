# System Architecture: Design system architecture

## System Components

- **ReportingDashboard.Web** — Blazor Server app hosting a single-page executive dashboard with milestone timeline, shipped/in-progress/carried-over sections, and project health summary.
- **data.json** — Flat JSON configuration file containing all project data (milestones, work items, metadata); read at startup and injected via a `DashboardDataService`.
- **DashboardPage.razor** — Single Razor component rendering the full dashboard, styled to match the OriginalDesignConcept.html template with improvements for executive readability.

## Data Model

- **ProjectInfo** — Project name, executive sponsor, reporting period, overall status (OnTrack/AtRisk/Blocked), summary narrative.
- **Milestone** — Id, title, target date, completion date, status (Completed/InProgress/Upcoming); displayed on the horizontal timeline.
- **WorkItem** — Id, title, description, category (Shipped/InProgress/CarriedOver), associated milestone id, owner, priority; grouped into dashboard sections.

## Project Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj
│       ├── Program.cs
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Pages/
│       │       └── Dashboard.razor
│       ├── Models/
│       │   ├── ProjectInfo.cs
│       │   ├── Milestone.cs
│       │   └── WorkItem.cs
│       ├── Services/
│       │   └── DashboardDataService.cs
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css
│       │   └── data/
│       │       └── data.json
│       └── Properties/
│           └── launchSettings.json
└── docs/
    └── OriginalDesignConcept.html
```

## Technology Choices

- **.NET 8 Blazor Server** — Interactive rendering with no WASM download; single-project simplicity for a local-only tool.
- **System.Text.Json** — Deserialize `data.json` at startup into strongly-typed models; no database or ORM needed.
- **Pure CSS** — Custom stylesheet derived from OriginalDesignConcept.html; no JavaScript frameworks, ensuring clean screenshot capture for PowerPoint decks.
- **Kestrel localhost only** — No authentication, no HTTPS certificates, no cloud dependencies; runs via `dotnet run` on `localhost:5000`.