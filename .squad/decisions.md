# Squad Decisions

## Active Decisions

### 2026-04-23: SVG text elements built via MarkupString
**By:** Implementation
**What:** Timeline SVG uses `MarkupString` string-building instead of inline Razor `<text>` elements because Blazor's Razor compiler interprets `<text>` as a Razor directive, not an SVG element.
**Why:** Avoids build error RZ1023 ("<text>" tags cannot contain attributes). All user data is HTML-encoded via `System.Net.WebUtility.HtmlEncode` before injection.

### 2026-04-23: Equal-width month columns in timeline
**By:** Implementation
**What:** Timeline gridlines use equal-width month columns (SVG_WIDTH / month_count) with milestone positions interpolated within their month column, rather than proportional total-day positioning.
**Why:** Matches the original design concept where months are evenly spaced at 260px intervals across the 1560px SVG.

### 2026-04-23: Heatmap cell styles in global CSS
**By:** Implementation
**What:** HeatmapCell.razor uses global CSS classes (`.hm-cell`, `.ship-cell`, etc.) from `app.css` instead of scoped CSS, because Blazor CSS isolation scopes to the component that renders the markup.
**Why:** HeatmapCell is a child of Heatmap.razor; scoped styles from Heatmap.razor.css won't apply to HeatmapCell's rendered elements.

### 2026-04-23: Interactive Server render mode
**By:** Implementation
**What:** Using Blazor Interactive Server (SignalR) rather than Static SSR.
**Why:** Required for FileSystemWatcher → OnDataChanged → InvokeAsync(StateHasChanged) live DOM updates without page refresh.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
