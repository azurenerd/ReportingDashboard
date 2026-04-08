# AgentSquad Executive Dashboard

Executive reporting dashboard built with C# .NET 8 Blazor Server for real-time visibility into project milestones, task status, and progress metrics. The dashboard reads data from a JSON configuration file, requires no authentication or cloud infrastructure, and prioritizes simplicity and screenshot-ready visuals for PowerPoint presentations.

## Project Purpose

AgentSquad provides a lightweight, single-page dashboard for executives to assess project health at a glance. Display your project's milestone timeline, task completion breakdown (Shipped/In-Progress/Carried-Over), and overall progress metrics—all from a local Blazor Server application with no external dependencies. Update project status by editing a simple `data.json` file; the dashboard refreshes on browser reload.

## Tech Stack

- **Framework:** C# .NET 8 Blazor Server (server-side rendering)
- **UI Framework:** Bootstrap 5.x (responsive grid, cards, progress components)
- **Data Format:** JSON (System.Text.Json for zero-dependency deserialization)
- **Storage:** File-based (wwwroot/data/data.json)
- **Charting:** Chart.js v4.x via JavaScript Interop (optional; can use SVG fallback)
- **Deployment:** Self-contained executable (Windows/Linux, no .NET SDK required on target machine)

## Key Features

- **Milestone Timeline:** Visual horizontal timeline with completion status indicators (Green/Blue/Gray)
- **Task Status Cards:** Three cards showing Shipped, In-Progress, and Carried-Over task counts
- **Progress Metrics:** Completion percentage and burn-down visualization
- **JSON-Driven:** Update data.json to refresh dashboard; no code redeployment needed
- **Responsive Design:** Optimized for 1024px+ desktop screens; screenshot-ready for PowerPoint
- **Error Handling:** Graceful error messages for missing/malformed JSON
- **Local-Only:** Zero cloud dependencies; runs entirely on your machine

## Quick Start

### Prerequisites

- .NET 8 SDK ([download](https://dotnet.microsoft.com/download))
- Git
- Code editor (VS Code, Visual Studio, or similar)

### 5-Minute Setup