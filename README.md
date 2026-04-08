# AgentSquad Dashboard

Executive reporting dashboard built with C# .NET 8 Blazor Server for displaying project milestones, task status, and progress metrics.

## Features

- **Milestone Timeline**: Visual timeline of project milestones with completion status
- **Task Status Cards**: Summary cards for Shipped, In-Progress, and Carried-Over tasks
- **Progress Metrics**: Overall project completion percentage and task breakdown
- **JSON-Based Configuration**: Load project data from data.json without database
- **Responsive Design**: Bootstrap 5 responsive layout optimized for desktop
- **Screenshot-Ready**: Deterministic rendering with no animations for PowerPoint integration

## Architecture

- **Dashboard.razor**: Main page component orchestrating three child components
- **MilestoneTimeline.razor**: Displays project milestones with status indicators
- **StatusCard.razor**: Shows task counts and lists by status category
- **ProgressMetrics.razor**: Displays completion percentage and progress bar
- **ProjectDataService**: Loads and validates project data from JSON

## Data Schema

The `wwwroot/data.json` file contains project information: