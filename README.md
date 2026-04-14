# Executive Reporting Dashboard

A single-page Blazor Server (.NET 8) dashboard that visualizes project milestones, delivery status, and monthly execution progress. Optimized for 1920×1080 PowerPoint screenshots.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4) ![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4) ![Local Only](https://img.shields.io/badge/Cloud-None-green)

## Overview

This tool generates a screenshot-ready project roadmap dashboard from a single `data.json` configuration file. It eliminates manual PowerPoint slide construction by rendering a pixel-perfect view directly in the browser.

**Key features:**
- 📊 SVG milestone timeline with configurable tracks and date markers
- 🗓️ Color-coded monthly execution heatmap (Shipped / In Progress / Carryover / Blockers)
- 🎯 Fixed 1920×1080 layout — screenshot and paste directly into PowerPoint
- ⚡ Update `data.json`, refresh browser, done in seconds
- 🔒 Runs entirely on localhost — zero cloud, zero auth, zero database

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (any 8.0.x patch version)
- A Chromium-based browser (Chrome 120+ or Edge 120+)
- Windows 10/11 recommended (for Segoe UI font rendering)

Verify your SDK installation: