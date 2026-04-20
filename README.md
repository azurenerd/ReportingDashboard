# Reporting Dashboard

A screenshot-optimized executive reporting dashboard rendered as a fixed 1920x1080 Blazor Server (Static SSR) page driven entirely by `wwwroot/data.json`.

See `PMSpec.md` and `Architecture.md` for the full product and design specification. The canonical visual reference is `OriginalDesignConcept.html` at the repo root.

## Getting Started

Prerequisites:

- .NET 8 SDK (8.0.x)
- Modern Chromium browser (Edge or Chrome) for viewing / screenshotting at 1920x1080

Run locally:

```bash
dotnet restore ReportingDashboard.sln
dotnet run --project src/ReportingDashboard.Web/ReportingDashboard.Web.csproj
```

Open <http://localhost:5080/> in the browser. The server binds to loopback only (no HTTPS, not reachable from the LAN).

## Editing data.json

The dashboard is a parameterized artifact - all content comes from `src/ReportingDashboard.Web/wwwroot/data.json`. Edit the file, refresh the browser, re-screenshot. The schema is defined by the POCOs under `src/ReportingDashboard.Web/Models/`.

## Tests

```bash
dotnet test ReportingDashboard.sln
```

## Screenshot Workflow

1. Open the page in Edge or Chrome on a 1920-wide display (or use DevTools device toolbar at 1920x1080).
2. Use Windows Snipping Tool (Win+Shift+S) to capture the viewport.
3. Paste into a 16:9 PowerPoint slide - no cropping required.

## Project Layout

```
ReportingDashboard.sln
src/ReportingDashboard.Web/    - Blazor Server app (Static SSR)
tests/ReportingDashboard.Web.Tests/ - xUnit + bUnit tests
```

## Security notes

This is a local-only, single-user tool. It binds to `http://localhost:5080` and has no authentication. Do not deploy to a shared server. Do not place PII in `data.json`.
