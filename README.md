# Executive Dashboard - Project Athena

A lightweight, single-page Blazor Server web application providing executives with real-time visibility into project health, milestones, and progress. Dashboard reads project data from a JSON configuration file, displays work items categorized by status, renders a timeline of key milestones, and supports screenshot extraction for PowerPoint presentations.

## Features

- **Real-Time Dashboard**: Single-page view of project health (<2 seconds load time)
- **Auto-Refresh on File Change**: FileSystemWatcher monitors data.json; page re-renders within 1 second
- **Milestone Timeline**: Custom SVG-based timeline with color-coded status indicators (Completed/On Track/At Risk)
- **Work Item Status**: Three color-coded sections (Shipped/In Progress/Carried Over) with item counts
- **Status Chart**: Bar/pie chart visualization of work item distribution
- **PowerPoint-Ready**: Fixed-width 1200px layout, light theme, professional typography; screenshot-ready at 1920x1080
- **No Authentication**: Internal-only tool; no login required
- **Zero Database**: File-based data storage (data.json); no external APIs

## Technology Stack

- **Framework**: .NET 8 Blazor Server (C#)
- **UI Components**: MudBlazor v7.0+ (MIT licensed)
- **Charting**: ChartJS.Blazor v4.1.2 (Chart.js wrapper)
- **JSON Parsing**: System.Text.Json (built-in .NET 8)
- **File Watching**: System.IO.FileSystemWatcher (built-in .NET)
- **Testing**: xUnit 2.6+, Moq 4.20+
- **Deployment**: Local machine or LAN server (HTTP, no HTTPS required)

## Requirements

- **.NET 8 SDK** (development) or .NET 8 Runtime (production)
- **Visual Studio 2022 (17.8+)** or VS Code + OmniSharp
- **Windows, Linux, or macOS**

## Quick Start

### 1. Clone & Build