# `data.json` Schema Reference

This annex is the authoritative **field-by-field** reference for `src/ReportingDashboard.Web/wwwroot/data.json`. The high-level overview and quick-start example live in the root `README.md`; this file owns the exhaustive table of types, defaults, required/optional flags, validation rules, and per-field examples.

The JSON shape maps 1:1 to the C# POCOs in `ReportingDashboard.Web.Models` (`DashboardData` and its children). Deserialization is done by `System.Text.Json` with the following options:

- `PropertyNameCaseInsensitive = true` &mdash; JSON keys may be `camelCase` (canonical) or `PascalCase`.
- `ReadCommentHandling = Skip` &mdash; `//` and `/* */` comments are tolerated.
- `AllowTrailingCommas = true` &mdash; trailing commas are tolerated.
- Enum values are case-insensitive strings (`JsonStringEnumConverter`).
- Dates are strict ISO-8601 calendar dates (`YYYY-MM-DD`), deserialized to `System.DateOnly`.

## VS Code IntelliSense

A JSON Schema (draft-07) ships at `src/ReportingDashboard.Web/wwwroot/data.schema.json`. Enable validation and completions by referencing it from the first property of your `data.json`:

```jsonc
{
  "$schema": "data.schema.json",
  "project": { ... }
}
```

Alternatively, add a project-level association in `.vscode/settings.json`:

```json
{
  "json.schemas": [
    {
      "fileMatch": ["**/wwwroot/data.json"],
      "url": "./src/ReportingDashboard.Web/wwwroot/data.schema.json"
    }
  ]
}
```

## Top-level object

| Field      | Type                       | Required | Default | Notes |
|------------|----------------------------|----------|---------|-------|
| `project`  | [`Project`](#project)      | yes      | &mdash; | Header-band content. |
| `timeline` | [`Timeline`](#timeline)    | yes      | &mdash; | SVG Gantt timeline model. |
| `heatmap`  | [`Heatmap`](#heatmap)      | yes      | &mdash; | Monthly execution grid. |
| `theme`    | [`Theme`](#theme)          | no       | `null`  | Reserved for future re-skin. Ignored in v1. |

Unknown top-level keys are ignored by `System.Text.Json` (the schema file rejects them, so IntelliSense will flag typos).

---

## `Project`

Maps to `ReportingDashboard.Web.Models.Project`.

| Field             | Type              | Required | Default             | Validation                                     |
|-------------------|-------------------|----------|---------------------|------------------------------------------------|
| `title`           | `string`          | yes      | &mdash;             | Non-empty after trim.                          |
| `subtitle`        | `string`          | yes      | &mdash;             | May be empty but must be present.              |
| `backlogUrl`      | `string \| null`  | no       | `null`              | If set, must be an absolute `http`/`https` URI (`Uri.TryCreate(..., Absolute)`); anything else degrades to an inert `<span>`. |
| `backlogLinkText` | `string`          | no       | `"\u2192 ADO Backlog"` | Free-text label used for the backlog anchor; Razor HTML-encodes the value. |

**Example**

```json
"project": {
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026",
  "backlogUrl": "https://dev.azure.com/contoso/privacy/_backlogs/backlog/",
  "backlogLinkText": "\u2192 ADO Backlog"
}
```

---

## `Timeline`

Maps to `ReportingDashboard.Web.Models.Timeline`. Defines the x-axis domain and swim lanes for the top SVG.

| Field   | Type                        | Required | Default | Validation |
|---------|-----------------------------|----------|---------|------------|
| `start` | `DateOnly` (`YYYY-MM-DD`)   | yes      | &mdash; | `start < end`. |
| `end`   | `DateOnly` (`YYYY-MM-DD`)   | yes      | &mdash; | `end > start`. |
| `lanes` | [`TimelineLane[]`](#timelinelane) | yes | &mdash; | 1..6 entries; `id` values unique. |

### `TimelineLane`

| Field        | Type                          | Required | Default | Validation |
|--------------|-------------------------------|----------|---------|------------|
| `id`         | `string`                      | yes      | &mdash; | Non-empty; unique across lanes (e.g. `"M1"`). |
| `label`      | `string`                      | yes      | &mdash; | Non-empty. |
| `color`      | `string` (hex RGB)            | yes      | &mdash; | Matches `^#[0-9A-Fa-f]{6}$`. |
| `milestones` | [`Milestone[]`](#milestone)   | yes      | &mdash; | Zero or more; order does not matter (layout engine sorts by date for caption placement). |

### `Milestone`

| Field              | Type                                  | Required | Default | Validation |
|--------------------|---------------------------------------|----------|---------|------------|
| `date`             | `DateOnly` (`YYYY-MM-DD`)             | yes      | &mdash; | Must lie within `[timeline.start, timeline.end]`. |
| `type`             | `"checkpoint" \| "poc" \| "prod"`     | yes      | &mdash; | Case-insensitive. Determines marker glyph and color. |
| `label`            | `string`                              | yes      | &mdash; | Caption rendered near the marker (e.g. `"Mar 26 PoC"`). |
| `captionPosition`  | `"above" \| "below" \| null`          | no       | `null`  | Optional override. If null/omitted, layout engine alternates placement to avoid collisions. |

**Marker glyphs**

| `type`       | Glyph                          | Fill       |
|--------------|--------------------------------|------------|
| `checkpoint` | Stroked white circle / grey dot | `#999` (grey) |
| `poc`        | 12&times;12 diamond            | `#F4B400` (amber) |
| `prod`       | 12&times;12 diamond            | `#34A853` (green) |

**Example**

```json
"timeline": {
  "start": "2026-01-01",
  "end":   "2026-06-30",
  "lanes": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12 Kickoff", "captionPosition": "above" },
        { "date": "2026-03-26", "type": "poc",        "label": "Mar 26 PoC",     "captionPosition": "below" },
        { "date": "2026-04-30", "type": "prod",       "label": "Apr Prod (TBD)" }
      ]
    }
  ]
}
```

---

## `Heatmap`

Maps to `ReportingDashboard.Web.Models.Heatmap`. A 4-row (fixed categories) &times; N-column (months) grid.

| Field                | Type                          | Required | Default | Validation |
|----------------------|-------------------------------|----------|---------|------------|
| `months`             | `string[]`                    | yes      | &mdash; | Length must match each row's `cells.Length`. v1 design uses exactly 4. |
| `currentMonthIndex`  | `int \| null`                 | no       | `null`  | If non-null, `0 <= value < months.Length`. If null, auto-computed from `DateTime.Today` matched against `months[]`; `-1` if no match. |
| `maxItemsPerCell`    | `int`                         | no       | `4`     | `>= 1`. Overflow becomes a single `"+K more"` item. |
| `rows`               | [`HeatmapRow[]`](#heatmaprow) | yes      | &mdash; | Exactly 4 entries covering the categories `shipped`, `inProgress`, `carryover`, `blockers` (order in JSON does not matter; render order is fixed). |

### `HeatmapRow`

| Field      | Type                                                | Required | Default | Validation |
|------------|-----------------------------------------------------|----------|---------|------------|
| `category` | `"shipped" \| "inProgress" \| "carryover" \| "blockers"` | yes | &mdash; | Case-insensitive. Categories must be unique across rows. |
| `cells`    | `string[][]`                                        | yes      | &mdash; | Outer length should equal `heatmap.months.length`. Each inner array is the list of item strings for the (category, month) cell; `[]` renders as a muted `"\u2013"`. |

**Category palette**

| Category     | Header text | Header bg  | Cell bg    | Current-month cell bg | Dot     |
|--------------|-------------|------------|------------|-----------------------|---------|
| `shipped`    | `#1B7A28`   | `#E8F5E9`  | `#F0FBF0`  | `#D8F2DA`             | `#34A853` |
| `inProgress` | `#1565C0`   | `#E3F2FD`  | `#EEF4FE`  | `#DAE8FB`             | `#0078D4` |
| `carryover`  | `#B45309`   | `#FFF8E1`  | `#FFFDE7`  | `#FFF0B0`             | `#F4B400` |
| `blockers`   | `#991B1B`   | `#FEF2F2`  | `#FFF5F5`  | `#FFE4E4`             | `#EA4335` |

**Example**

```json
"heatmap": {
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": null,
  "maxItemsPerCell": 4,
  "rows": [
    {
      "category": "shipped",
      "cells": [
        ["Chatbot v0.9 dev build", "DFD schema v1"],
        ["PDS inventory baseline"],
        [],
        ["Privacy banner rollout"]
      ]
    },
    {
      "category": "inProgress",
      "cells": [[], [], ["Role-based redaction"], ["Chatbot prod hardening", "Review queue UX", "SLA dashboards"]]
    },
    {
      "category": "carryover",
      "cells": [[], ["Legacy API migration"], ["Legacy API migration", "Vendor DPA refresh"], ["Vendor DPA refresh"]]
    },
    {
      "category": "blockers",
      "cells": [[], [], ["Vendor SLA pending"], ["Vendor SLA pending", "Capacity review gate"]]
    }
  ]
}
```

---

## `Theme` (reserved)

Maps to `ReportingDashboard.Web.Models.Theme`. Present as an extensibility hook; **not rendered** in v1.

| Field  | Type              | Required | Default | Notes |
|--------|-------------------|----------|---------|-------|
| `font` | `string \| null`  | no       | `null`  | Future font-family override. Ignored today. |

---

## Validation error kinds

When `DashboardDataService` rejects a file, the in-page red banner's `Kind` reflects the failure mode:

| `Kind`             | Cause                                                             |
|--------------------|-------------------------------------------------------------------|
| `NotFound`         | `data.json` missing at the resolved path.                         |
| `ParseError`       | `JsonException` from `System.Text.Json` (bad syntax, wrong type). |
| `ValidationError`  | One or more rules above failed (`DashboardDataValidator`).        |

See `Architecture.md` &sect; C2/C3 for the service contract and the enforced rule list.

---

## Canonical example

The repository ships a fully populated `data.json` at `src/ReportingDashboard.Web/wwwroot/data.json`. Use it as the starting point for new projects &mdash; copy, swap the content, and refresh the browser.
