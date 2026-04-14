# Executive Reporting Dashboard

A single-page Blazor Server (.NET 8) dashboard that renders a pixel-perfect 1920×1080 project roadmap visualization — optimized for PowerPoint screenshot capture. All content is driven by a single `data.json` configuration file.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-purple)
![Local Only](https://img.shields.io/badge/Hosting-Localhost-green)

---

## Features

- **Fixed 1920×1080 layout** — screenshot-ready for 16:9 PowerPoint slides
- **SVG milestone timeline** — configurable tracks (1–5) with PoC, Production, and Checkpoint markers
- **Execution heatmap** — color-coded grid showing Shipped, In Progress, Carryover, and Blockers by month
- **Single data source** — edit `data.json`, refresh the browser, done
- **Zero dependencies** — no NuGet packages beyond the default Blazor Server template
- **Graceful error handling** — malformed JSON displays a friendly error instead of crashing

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (any 8.0.x patch version)
- A Chromium-based browser (Chrome 120+ or Edge 120+)
- Windows 10/11 recommended (Segoe UI font availability)

Verify your .NET SDK installation: