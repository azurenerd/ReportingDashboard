# Reporting Dashboard

Single-page, screenshot-optimized executive reporting dashboard (Blazor Server,
.NET 8, Static SSR, local only). The rendered page at 1920x1080 is designed to be
screenshotted directly into a 16:9 PowerPoint slide.

## Getting Started

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (8.0.400+ recommended).
2. `git clone` this repo and `cd` into it.
3. `dotnet run --project src/ReportingDashboard.Web`
4. Open <http://localhost:5080/> in Edge or Chrome at a 1920-wide viewport.
5. Edit `src/ReportingDashboard.Web/wwwroot/data.json` and refresh the browser to see changes (hot-reload, no restart).

## No Authentication (by design)

This is a **local-only, single-user screenshot utility**. Kestrel binds to
`http://localhost:5080` (loopback) and there is no authentication, no HTTPS, and
no outbound network calls at runtime. Do **not** deploy to a shared host or
expose the port to a network. Do not put PII or secrets in `data.json`.

## `data.json` - the only source of truth

The dashboard renders entirely from one hand-edited JSON file:

- **Location:** `src/ReportingDashboard.Web/wwwroot/data.json`
- **Encoding:** UTF-8 (BOM tolerated). Trailing commas and `//` comments are allowed.
- **Hot-reload:** saved changes are picked up within ~250ms via `FileSystemWatcher`; refresh the browser to see them.
- **Errors:** a malformed or missing file renders a red banner at the top of the page and the rest of the dashboard degrades to empty placeholders (no crash).

### Annotated schema

Every field used by the renderer is shown below. The shipped `data.json` is a
working example you can copy and edit.

```jsonc
{
  // --- Project header (DashboardHeader.razor) -----------------------------
  "project": {
    "title":    "Privacy Automation Release Roadmap",          // required, 24px bold in header
    "subtitle": "Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026",
                                                               // required, 12px #888 under the title
    "backlogUrl": "https://dev.azure.com/.../backlog/"         // optional; renders inline "-> ADO Backlog" link. Omit/null -> plain text.
  },

  // --- Milestone timeline (TimelineSvg.razor) -----------------------------
  "timeline": {
    "start": "2026-01-01",   // required, ISO date (DateOnly). Must be < end.
    "end":   "2026-06-30",   // required, ISO date (DateOnly).

    // 1..6 lanes. Rendered top-to-bottom on the left lane-label column and as
    // horizontal SVG tracks on the right.
    "lanes": [
      {
        "id":    "M1",                   // required, unique across lanes (short code)
        "label": "Chatbot & MS Role",    // required, long name shown under the id
        "color": "#0078D4",              // required, #RRGGBB - lane track stroke + id color
        "milestones": [
          {
            "date":  "2026-03-26",       // required, must fall within [start, end]
            "type":  "poc",              // required: "poc" | "prod" | "checkpoint"
                                         //   poc        -> amber diamond (#F4B400)
                                         //   prod       -> green diamond (#34A853)
                                         //   checkpoint -> grey circle (#999)
            "label": "Mar 26 PoC",       // required, 10px caption above/below the marker
            "captionPosition": "Above"   // optional: "Above" | "Below". Omit to let the layout engine decide.
          }
          // ... zero or more additional milestones
        ]
      }
      // ... up to 5 more lanes
    ]
  },

  // --- Monthly execution heatmap (Heatmap.razor) --------------------------
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],   // required. Column headers, left-to-right. v1 default length is 4.

    // Optional. Zero-based index of the "current" month column (gets the
    // amber #FFF0D0 header + darker cell tint). Set to null to auto-compute
    // from DateTime.Today against `months` (matching by abbreviation).
    "currentMonthIndex": null,

    // Optional, default 4. A cell with more items than this renders the first
    // (N-1) items verbatim and collapses the rest into one "+K more" row.
    "maxItemsPerCell": 4,

    // Exactly 4 rows. Categories fixed; order in rendering is always:
    // Shipped -> In Progress -> Carryover -> Blockers.
    "rows": [
      {
        "category": "shipped",    // required: "shipped" | "inProgress" | "carryover" | "blockers"
        "cells": [                // required. Length MUST equal months.length (4).
          ["Item A", "Item B"],   //   cell = array of plain-text strings (HTML-encoded by Razor)
          [],                     //   empty array -> cell shows a grey "-" placeholder
          ["X"],
          ["Y", "Z"]
        ]
      },
      { "category": "inProgress", "cells": [ /* 4 arrays */ ] },
      { "category": "carryover",  "cells": [ /* 4 arrays */ ] },
      { "category": "blockers",   "cells": [ /* 4 arrays */ ] }
    ]
  }
}
```

Validation rules enforced on load (errors render in the red banner, not a stack trace):

- `project.title` non-empty; `backlogUrl` parses as an absolute `http`/`https` URI (or is omitted).
- `timeline.start < timeline.end`; 1..6 lanes; unique `id`; color matches `^#[0-9A-Fa-f]{6}$`.
- Every `milestone.date` falls in `[start, end]`; `type` is `poc` | `prod` | `checkpoint`.
- `heatmap.rows` has exactly 4 rows; each row's `cells.length == months.length`.
- `heatmap.currentMonthIndex` is either `null` or in `[0, months.length)`.
- `heatmap.maxItemsPerCell`, when set, is an integer >= 1.

## Screenshot Workflow

The page is fixed at **1920x1080** with `overflow:hidden` - canonical deck
screenshots require a 1920-wide viewport.

1. Open the dashboard in **Edge or Chrome** on Windows (Segoe UI is the canonical font).
2. Use either approach to get a clean 1920x1080 viewport:
   - **External monitor / native window** sized to at least 1920 wide, or
   - Press **F12 -> device toolbar** (`Ctrl+Shift+M`), select **Responsive**, and type `1920 x 1080`. Zoom 100%.
3. Press **`Win+Shift+S`** to open the Windows Snipping Tool, choose **Rectangular snip**, and drag across the full 1920x1080 viewport.
4. Paste (`Ctrl+V`) directly into a 16:9 PowerPoint slide - no cropping or resizing needed.

Do not screenshot at reduced browser zoom (e.g., 75%) for deck use - fonts
and SVG strokes will not hit their pixel-faithful weights. Zoom is fine for
on-screen review only.

## Optional: Self-Contained Publish

To hand a double-clickable `.exe` to a PM who does not have the .NET SDK:

```powershell
dotnet publish src/ReportingDashboard.Web `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish/win-x64
```

The output is `publish/win-x64/ReportingDashboard.Web.exe` (~70 MB) alongside a
`wwwroot/` folder. `data.json` stays external and PM-editable. Launch the
executable and open <http://localhost:5080/> manually.
