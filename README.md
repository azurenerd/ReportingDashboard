# My Project - Reporting Dashboard

A minimal, local-only Blazor Server (.NET 8) app that renders a fixed **1920x1080** executive
status dashboard (header + legend, milestone timeline, monthly execution heatmap) from a
single hand-edited `data.json`. The page exists to be **screenshotted into a PowerPoint slide** -
no BI tool, no auth, no cloud, no interactivity.

Canonical visual reference: [`OriginalDesignConcept.html`](OriginalDesignConcept.html) and
[`docs/design-screenshots/OriginalDesignConcept.png`](docs/design-screenshots/OriginalDesignConcept.png).

---

## Getting Started

```bash
git clone <repo-url>
cd src/ReportingDashboard.Web
dotnet run
# open http://localhost:5080
```

> Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (8.0.x). No other
> prerequisites - Kestrel binds only to `http://localhost:5080` (loopback, no HTTPS).

---

## Editing `data.json`

### Path

The dashboard reads **one** file:

```
src/ReportingDashboard.Web/wwwroot/data.json
```

You can also view the currently-served file in a browser at
[`http://localhost:5080/data.json`](http://localhost:5080/data.json) to sanity-check what
the app is actually parsing.

### Hot-reload behavior

- A `FileSystemWatcher` watches `wwwroot/data.json` and debounces change events by ~250ms.
- On save, the in-memory cache is invalidated - **no process restart required**.
- The browser does **not** auto-refresh (there is no SignalR circuit in static SSR).
  Workflow: **save the file**, then **refresh the browser** (F5). Your edits appear
  immediately.
- If the file is missing, malformed JSON, or fails schema validation, the page renders a
  red error banner at the top (with file path, line/column if available, and the parse
  message) instead of crashing. Fix the file, save, refresh - the banner goes away.

### Annotated schema

```jsonc
{
  // ----- Project: header band content -----
  "project": {
    "title":           "Privacy Automation Release Roadmap",        // h1, 24px bold
    "subtitle":        "Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026",
                                                                    // 12px muted grey under title
    "backlogUrl":      "https://dev.azure.com/contoso/privacy/_backlogs/backlog/",
                                                                    // optional; must be http/https
                                                                    //   or it is rendered as plain text
    "backlogLinkText": "\u2192 ADO Backlog"                         // optional; default "\u2192 ADO Backlog"
  },

  // ----- Timeline: 196px-tall Gantt-style SVG with lanes + milestones + NOW line -----
  "timeline": {
    "start": "2026-01-01",                                          // ISO date (yyyy-MM-dd), inclusive
    "end":   "2026-06-30",                                          // ISO date, must be > start
    "lanes": [                                                      // 1..6 lanes
      {
        "id":    "M1",                                              // short code shown in the left label column
        "label": "Chatbot & MS Role",                               // longer sub-label
        "color": "#0078D4",                                         // lane track + id color; #RRGGBB only
        "milestones": [
          {
            "date":  "2026-01-12",                                  // must be within [timeline.start, timeline.end]
            "type":  "checkpoint",                                  // "checkpoint" | "poc" | "prod"
                                                                    //   checkpoint -> grey circle / outlined dot
                                                                    //   poc        -> amber diamond (#F4B400)
                                                                    //   prod       -> green diamond (#34A853)
            "label": "Jan 12 Kickoff",                              // caption rendered next to the marker
            "captionPosition": "above"                              // optional: "above" | "below"
                                                                    //   (auto-alternates when omitted)
          },
          { "date": "2026-03-26", "type": "poc",  "label": "Mar 26 PoC",     "captionPosition": "below" },
          { "date": "2026-04-30", "type": "prod", "label": "Apr Prod (TBD)", "captionPosition": "above" }
        ]
      },
      { "id": "M2", "label": "PDS & Data Inventory", "color": "#00897B", "milestones": [ /* ... */ ] },
      { "id": "M3", "label": "Auto Review DFD",      "color": "#546E7A", "milestones": [ /* ... */ ] }
    ]
  },

  // ----- Heatmap: 4x4 grid of monthly execution items -----
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],                         // column headers; length must equal
                                                                    //   each row's cells.length (default 4)
    "currentMonthIndex": null,                                      // 0-based index of the "current" column
                                                                    //   (amber header + darker cell tint);
                                                                    //   null => auto from today's month
                                                                    //   (matched by abbreviation against months[])
    "maxItemsPerCell": 4,                                           // truncate at N; overflow becomes "+K more"
    "rows": [                                                       // exactly 4 rows, in the order below
      {
        "category": "shipped",                                      // "shipped"    -> green  dot #34A853
        "cells": [                                                  //   one array per month (length == months.length)
          ["Chatbot v0.9 dev build", "DFD schema v1"],              //   each string is an item line in that cell
          ["PDS inventory baseline", "Role registry export"],
          ["Chatbot PoC demo", "Auto Review rules v1"],
          ["Privacy banner rollout", "Consent API v1.2"]
        ]
      },
      { "category": "inProgress", "cells": [ /* ... */ ] },         // "inProgress" -> blue   dot #0078D4
      { "category": "carryover",  "cells": [ /* ... */ ] },         // "carryover"  -> amber  dot #F4B400
      { "category": "blockers",   "cells": [ /* ... */ ] }          // "blockers"   -> red    dot #EA4335
                                                                    // Empty cell ([]) renders a muted "-".
    ]
  },

  // ----- Theme: reserved for future re-skinning; unused in v1 -----
  "theme": { "font": "Segoe UI" }
}
```

**Validation rules enforced at load time:**

- `project.title` non-empty; `backlogUrl` must be an absolute `http`/`https` URL (else the link
  degrades to plain text).
- `timeline.start < timeline.end`; 1..6 lanes; each lane `id` unique; `color` matches
  `#RRGGBB`.
- Every `milestone.date` lies within `[start, end]`; `type`  { `checkpoint`, `poc`, `prod` };
  `captionPosition`  { `above`, `below` } when present.
- `heatmap.months.length == rows[i].cells.length` (default 4).
- Exactly 4 rows with categories `shipped`, `inProgress`, `carryover`, `blockers`
  (case-insensitive).
- `currentMonthIndex` is either `null` or in `[0, months.length)`.
- `maxItemsPerCell` is an integer `>= 1` (default `4`).

A complete working sample ships in the repo at
[`src/ReportingDashboard.Web/wwwroot/data.json`](src/ReportingDashboard.Web/wwwroot/data.json) -
use it as your starting point.

---

## Screenshot Workflow

The page is intentionally a fixed **1920x1080** artifact with no scrollbars, no spinners,
and no chrome. To produce a deck-ready screenshot:

1. **Use Windows + Edge or Chrome.** Segoe UI is the canonical font; other OSes fall back to
   similar sans-serif fonts and text metrics will shift slightly.
2. **Get a 1920-wide viewport.** Either:
   - Plug into an external monitor at 1920+ px wide and maximize the browser, **or**
   - Open DevTools (`F12`) -> **Toggle device toolbar** (`Ctrl+Shift+M`) -> set the viewport
     to `1920 x 1080` -> reload.
3. **Capture the viewport.** `Win+Shift+S` (Snipping Tool) -> **Window** or **Rectangular**
   capture covering the full 1920x1080 frame.
4. **Paste into a 16:9 PowerPoint slide.** No cropping or resizing needed.

> Laptop screens narrower than 1920px will horizontally clip the body. Browser zoom
> (e.g. 75%) lets you *preview* the dashboard at reduced size, but screenshots taken at
> reduced zoom are **not** deck-ready - always capture at 100% zoom, 1920-wide viewport.

---

## Project Layout

```
ReportingDashboard.sln
 src/ReportingDashboard.Web/        # Blazor Server app (static SSR)
    wwwroot/data.json              # <-- the only data source; edit this
    Components/Pages/               # Dashboard + partials (Heatmap, ...)
    Models/                         # DashboardData, Timeline, Heatmap, ...
    Services/                       # DashboardDataService (+ validator, load result)
    Layout/                         # HeatmapLayoutEngine, view models
    Program.cs                      # minimal host, binds http://localhost:5080
 tests/                              # xUnit + bUnit
 docs/design-screenshots/           # canonical PNG reference
 OriginalDesignConcept.html          # canonical HTML/CSS reference
```

## Build & Test

```bash
dotnet restore ReportingDashboard.sln
dotnet build   ReportingDashboard.sln -c Release
dotnet test    ReportingDashboard.sln -c Release
```

## Optional: self-contained publish

Produce a double-clickable `.exe` (plus an editable `wwwroot/data.json` alongside):

```bash
dotnet publish src/ReportingDashboard.Web `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o publish/win-x64
```

Run `publish/win-x64/ReportingDashboard.Web.exe` and open
<http://localhost:5080>.

---

## Design Principles

- **Static SSR only.** No `@rendermode InteractiveServer` / `InteractiveWebAssembly`, no
  SignalR circuit, no reconnect UI - nothing that could land in a screenshot.
- **Single source of truth.** Every rendered string, date, color, and coordinate flows from
  `data.json`. Nothing is hardcoded in Razor.
- **Fail-safe rendering.** Bad data produces an in-page banner, never a YSOD.
- **Local only.** No auth, no HTTPS, no cloud. Do **not** place PII or secrets in `data.json`
  and do **not** deploy this to a shared server without revisiting the security model.
