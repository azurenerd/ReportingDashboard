# Build & Verification Steps

## 1. Merge CSS (delete dashboard.css.append, append its content to dashboard.css)

The error panel CSS from `dashboard.css.append` must be appended to the bottom of
`src/ReportingDashboard/wwwroot/css/dashboard.css`. Then delete the `.append` file.

Append this block to the end of dashboard.css:

/* ===== Error Panel ===== */
.error-panel { display:flex; align-items:center; justify-content:center; width:1920px; height:1080px; background:#FFFFFF; font-family:'Segoe UI',Arial,sans-serif; }
.error-panel > div { text-align:center; max-width:600px; }
.error-icon { font-size:48px; color:#EA4335; margin-bottom:16px; }
.error-title { font-size:20px; font-weight:700; color:#333; margin-bottom:12px; }
.error-details { font-size:14px; color:#666; font-family:monospace; margin-bottom:16px; word-break:break-all; }
.error-help { font-size:12px; color:#888; line-height:1.6; }

## 2. Build

dotnet build src/ReportingDashboard/ReportingDashboard.csproj

Expected: Build succeeded. 0 Warning(s). 0 Error(s).

## 3. Verify Scenario A — Valid data.json

cd src/ReportingDashboard && dotnet run
Open http://localhost:5000 — dashboard renders Header, Timeline, Heatmap. No error-panel div present.

## 4. Verify Scenario B — Missing data.json

Rename wwwroot/data.json to data.json.bak, restart app.
Browser shows centered error panel with text: "data.json not found at {path}"
Console logs the same message.
Restore: rename data.json.bak back to data.json.

## 5. Verify Scenario C — Malformed data.json

Replace wwwroot/data.json content with: { "title": INVALID }
Restart app.
Browser shows error panel with text: "Failed to parse data.json: ..."
Console logs the parse error details.
Restore valid data.json.