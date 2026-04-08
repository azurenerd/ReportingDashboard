# Executive Project Reporting Dashboard

A lightweight, screenshot-optimized executive dashboard built with Blazor Server that reads project milestones and progress data from a JSON configuration file.

## Quick Start

### Prerequisites
- .NET 8 SDK installed
- Chrome or Edge browser (latest 2 versions)
- Minimum display resolution: 1024x768 (optimized for 1920x1080)

### Development Setup

1. Clone the repository
2. Create or copy `wwwroot/data.json` with valid project data (see schema below)
3. Open terminal in project root
4. Run: `dotnet run`
5. Browser opens to `https://localhost:5001` (HTTP: `http://localhost:5000`)

### Production Deployment

**Self-Contained Executable (Windows x64)**