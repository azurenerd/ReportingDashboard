# Executive Reporting Dashboard

## Project Overview

A simple, single-page Executive Reporting Dashboard built with Blazor Server (.NET 8) that visualizes project milestones, progress, and status for executive stakeholders. The dashboard reads data from a JSON file (`data.json`), enables real-time auto-refresh via FileSystemWatcher, and produces clean, PowerPoint-ready screenshots.

**Key Features:**
- One-click installation (self-contained .exe, no IT setup required)
- Real-time dashboard updates when JSON file is edited
- Print-optimized CSS for professional PowerPoint screenshots
- Zero external dependencies (localhost-only, no cloud services)
- CEO-editable JSON schema (edit in VS Code, Notepad, etc.)

**Technology Stack:**
- C# .NET 8.0 LTS
- Blazor Server (server-side rendering)
- Bootstrap 5.3 (CDN)
- Chart.js 4.4 (CDN, horizontal bar charts)
- System.Text.Json (built-in)

---

## System Requirements

**Operating System:**
- Windows 10+ (64-bit, x86-64 architecture)
- Self-contained .exe bundles .NET 8 runtime (no pre-installation required)

**Hardware:**
- CPU: 1 GHz or faster (x64 processor)
- RAM: 512 MB minimum, 1 GB recommended
- Disk: 300 MB free space

**Browser:**
- Chrome, Edge, or Firefox (latest stable)
- Localhost access to http://127.0.0.1:5000

**Network (Optional):**
- CDN access (jsDelivr) for Bootstrap 5.3 and Chart.js on first load
- Fallback styling if CDN unavailable
- App works fully offline after first load

---

## Setup Instructions (5 Minutes)

### 1. Download & Launch
1. Download `ReportingDashboard.exe` from intranet/email link
2. **Double-click the .exe file**
3. App automatically:
   - Starts Kestrel web server (localhost:5000)
   - Opens default browser to http://127.0.0.1:5000
   - Loads sample dashboard with data from `data.json`

### 2. Verify Installation
- Dashboard displays "Horizon Platform Delivery" project
- 4 milestones visible (Phase 1-4) with progress bars
- Status columns show Shipped, In Progress, Carried Over items
- KPI metrics display: onTimeDelivery, teamCapacity, codeQuality, testCoverage
- Refresh timestamp at bottom shows current time

### 3. First Edit (1 Minute)
1. Open `data.json` in VS Code or Notepad (same directory as .exe)
2. Change `"progress": 85` to `"progress": 90` (first milestone)
3. Save file (Ctrl+S)
4. Dashboard automatically updates within 1 second
5. Refresh timestamp updates to current time

---

## data.json Schema Reference

### Structure