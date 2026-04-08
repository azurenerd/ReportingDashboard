# Executive Dashboard

A lightweight, single-page executive reporting dashboard built with C# .NET 8 and Blazor Server. Displays project milestones, task status, and progress metrics with screenshot-ready visuals for PowerPoint integration.

## Features

- **Task Status Breakdown**: Three color-coded cards display Shipped (green), In-Progress (blue), and Carried-Over (orange) tasks
- **Expandable Task Lists**: Each status card contains an expandable list with task names and assigned owners
- **JSON-Based Data**: All data loaded from `wwwroot/data.json` without database backend
- **Responsive Design**: Optimized for desktop (1024px+) and mobile screens
- **Screenshot Ready**: Deterministic rendering without animations for consistent PowerPoint capture
- **Error Handling**: User-friendly error messages for malformed JSON or missing data

## Project Structure