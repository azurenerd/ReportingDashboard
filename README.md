# AgentSquad Executive Dashboard

## Project Overview

AgentSquad is a lightweight executive reporting dashboard built with C# .NET 8 Blazor Server that displays project milestones, task status, and progress metrics through a single-page interface. The dashboard reads project data from a JSON configuration file, requires no external authentication or cloud dependencies, and prioritizes screenshot-ready visuals for PowerPoint integration.

**Key Features:**
- Visual timeline of project milestones with completion status indicators
- Task status breakdown cards (Shipped, In-Progress, Carried-Over) with expandable task lists
- Project completion percentage and burn-down metrics visualization
- Responsive Bootstrap 5 grid layout supporting desktop screens 1024px and wider
- File-based data storage (data.json) for manual updates without code redeployment
- Graceful error handling with user-friendly messages for malformed JSON

**Technology Stack:** Blazor Server (C# .NET 8), Bootstrap 5, Chart.js v4, System.Text.Json

**Target Users:** Executives, project managers, and stakeholders who need quick visibility into project health for status briefings.

## Quick Start

Follow these 5 commands to clone, build, and run the dashboard:

1. Clone the repository: