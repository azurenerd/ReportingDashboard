# System Architecture: Design system architecture

## System Components

- **ReportingDashboard.Web**: Blazor Server application serving a single-page executive report with milestone timeline, shipped items, in-progress work, and carryover sections.
- **data.json**: Static JSON configuration file in `wwwroot/data/` containing all project reporting data — no database required.
- **ReportDashboard.razor**: Main (and only) page component rendering the full executive view, styled to match `OriginalDesignConcept.html` and `ReportingDashboardDesign.png`.

## Data Model

- **ProjectReport**: Root entity with `ProjectName`, `ReportDate`, `ExecutiveSummary`, and `OverallStatus` (OnTrack/AtRisk/Blocked).
- **Milestone**: `Name`, `TargetDate`, `Status`, `Description` — rendered in horizontal timeline at page top.
- **WorkItem**: `Title`, `Description`, `Owner`, `Status`, `Category` (Shipped/InProgress/CarriedOver), `Priority` — grouped into three card sections.
- **StatusUpdate**: `Date`, `Summary`, `Risks[]`, `NextSteps[]` — displayed as the latest status narrative block.

## Project Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
└── src/
    └── ReportingDashboard.Web/
        ├── ReportingDashboard.Web.csproj
        ├── Program.cs
        ├── Models/
        │   ├── ProjectReport.cs
        │   ├── Milestone.cs
        │   ├── WorkItem.cs
        │   └── StatusUpdate.cs
        ├── Services/
        │   └── ReportDataService.cs
        ├── Components/
        │   ├── Pages/
        │   │   └── ReportDashboard.razor
        │   ├── Layout/
        │   │   └── MainLayout.razor
        │   ├── MilestoneTimeline.razor
        │   └── WorkItemCard.razor
        └── wwwroot/
            ├── css/
            │   └── dashboard.css
            └── data/
                └── data.json
```

## Technology Choices

- **Blazor Server (.NET 8)**: Enables C# full-stack with hot reload; no JavaScript frameworks needed. Targets `net8.0` with `Microsoft.AspNetCore.Components.Web`.
- **System.Text.Json**: Deserializes `data.json` at startup via `ReportDataService`, registered as a singleton.
- **Pure CSS**: Custom stylesheet matching the original HTML design — no Tailwind/Bootstrap — ensuring clean, screenshot-friendly output for PowerPoint decks.