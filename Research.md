# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-07 19:33 UTC_

### Summary

Building a local-only Blazor Server application with C# .NET 8 is the optimal approach for executive project milestone reporting. The stack combines server-side rendering with OxyPlot for Gantt timelines and Chart.js for trend visualization, eliminating JavaScript complexity while leveraging existing C# expertise. Local file-based JSON data loading with weekly snapshot persistence provides sufficient historical context for trend analysis. This approach balances simplicity, maintainability, and rich executive UX without enterprise infrastructure overhead. ---

### Key Findings

- **Gantt/timeline visualization** is non-negotiable for executive dashboards; OxyPlot 2.1.2 is the only production-ready .NET 8 library providing this natively.
- **Blazor Server** (not WebAssembly) eliminates JavaScript entirely, accelerates development velocity, and simplifies local deployment to a single self-contained .exe file.
- **Data schema** must prioritize milestone-centric hierarchy (timelines first) with flat work-item references for flexible filtering and categorization (shipped/in-progress/carried-over).
- **System.Text.Json** (built into .NET 8) eliminates external JSON dependencies; in-memory caching at startup avoids per-request disk I/O.
- **File-based snapshots** (weekly JSON dumps to `data/snapshots/`) provide historical trend visibility without SQLite complexity; typical 12-week history requires <3MB storage locally.
- **Responsive design via Tailwind CSS v3.4.1** and server-side media queries handles mobile/tablet access without JavaScript frameworks; OxyPlot and Chart.js both support responsive rendering.
- **PDF export** via QuestPDF v2024.3 enables executive-friendly reports; server-side generation avoids client-side library dependencies.
- **Development/deployment complexity** is minimal: single Blazor Server process with embedded Kestrel, self-contained .NET 8 runtime, file I/O only (no external services/databases).
- ---
- [ ] Create Blazor Server project (.NET 8)
- [ ] Load data.json via `ProjectDataService`; bind to Razor components
- [ ] Build `MilestoneTimeline.razor` with OxyPlot Gantt (3-4 sample milestones)
- [ ] Build three `StatusBucket` components (shipped/in-progress/carried-over) with sample work items
- [ ] Style with Tailwind CSS for basic executive look
- [ ] Deploy as self-contained .exe; test on local machine
- [ ] Demo to stakeholder; gather visual/content feedback
- [ ] Integrate Chart.js for velocity/burndown trends (line charts)
- [ ] Build `ExecutiveSummary.razor` with KPI badges (velocity %, burn rate, blocker count)
- [ ] Implement responsive Tailwind layout (desktop/tablet/mobile breakpoints)
- [ ] Add "Refresh" button with manual reload from disk
- [ ] Implement FileSystemWatcher for auto-detection of data.json changes
- [ ] Build basic logging (file-based; errors logged to `logs/` folder)
- [ ] Implement `SnapshotService`; create weekly scheduled snapshot job
- [ ] Build history queries to compute trends (velocity over past 12 weeks)
- [ ] Integrate QuestPDF; build executive PDF export (one-click download)
- [ ] Implement retention policy (keep last 52 snapshots; delete older)
- [ ] Add unit tests for `ProjectDataService` and `SnapshotService`
- [ ] Refine Gantt timeline aesthetics (colors, fonts, legend)
- [ ] Performance optimization: profile OxyPlot rendering; optimize for 1000+ items if needed
- [ ] Documentation: README with setup, data.json format, deployment instructions
- [ ] Package as Windows Service (optional) or scheduled task for auto-launch
- [ ] UAT with executive stakeholders
- [ ] Deploy to production (executive machine or shared network location)
- **Week 1:** Load data.json + render simple milestone list → Proves data pipeline works; executive sees something real
- **Week 2:** OxyPlot Gantt + three work-item buckets → Core executive view; minimal but complete
- **Week 2.5:** Tailwind responsive styling → "Looks like a real app"
- **Week 3:** Chart.js trend line → "This is trending the right way/wrong way" executive insight
- **OxyPlot Gantt rendering:** Create proof-of-concept with 5-6 milestones; validate SVG export quality and performance
- **Chart.js interop:** Test IJSRuntime invocation with simple bar chart; confirm WebSocket data flow works
- **QuestPDF rendering:** Generate sample PDF; confirm styling matches web view
- Visual Studio 2022 (Community edition sufficient) or Visual Studio Code + dotnet CLI
- .NET 8 SDK (download from microsoft.com)
- Git for version control
- ```bash
- dotnet new blazorserver -n ExecutiveDashboard
- cd ExecutiveDashboard
- dotnet add package OxyPlot.Core --version 2.1.2
- dotnet add package QuestPDF --version 2024.3.0
- dotnet add package FluentValidation --version 11.9.1
- dotnet run
- ```
- ```bash
- dotnet build                    # Compile
- dotnet test                     # Run xUnit tests
- dotnet run                      # Local Kestrel (http://localhost:5000)
- dotnet publish -c Release --self-contained --runtime win-x64 -p:PublishSingleFile=true  # Final .exe
- ```
- ---
- ```json
- {
- "projectName": "Cloud Infrastructure Migration",
- "reportingPeriod": "2026-04",
- "milestones": [
- {
- "id": "m1",
- "name": "AWS Infrastructure Foundation",
- "dueDate": "2026-04-15",
- "completionDate": null,
- "status": "in-progress",
- "priority": "critical"
- },
- {
- "id": "m2",
- "name": "Database Migration",
- "dueDate": "2026-05-01",
- "completionDate": null,
- "status": "planned",
- "priority": "critical"
- }
- ],
- "workItems": [
- {
- "id": "w1",
- "title": "VPC Configuration",
- "category": "in-progress",
- "milestone": "m1",
- "dueDate": "2026-04-10",
- "completionDate": null,
- "estimatedHours": 40,
- "actualHours": 35,
- "assignee": "Alice Chen"
- },
- {
- "id": "w2",
- "title": "Security Group Rules",
- "category": "shipped",
- "milestone": "m1",
- "dueDate": "2026-04-08",
- "completionDate": "2026-04-07",
- "estimatedHours": 24,
- "actualHours": 22,
- "assignee": "Bob Smith"
- }
- ],
- "metrics": {
- "velocity": 12,
- "burndownRate": 0.85,
- "blockers": ["Waiting for security approval", "AWS account provisioning delayed"]
- }
- }
- ```

### Recommended Tools & Technologies

- | Component | Technology | Version | Purpose |
- |-----------|-----------|---------|---------|
- | **Web Framework** | Blazor Server | .NET 8 | Server-side rendering + component model |
- | **Timeline/Gantt Chart** | OxyPlot.Core | 2.1.2 | Executive milestone visualization |
- | **Trend Chart** | Chart.js (via CDN) | 4.4.0 | Velocity/burndown line charts |
- | **CSS Framework** | Tailwind CSS | 3.4.1 | Responsive utilities-first styling |
- | **Interop Bridge** | IJSRuntime | .NET 8 built-in | Chart.js JavaScript invocation |
- | Component | Technology | Version | Purpose |
- |-----------|-----------|---------|---------|
- | **Runtime** | .NET | 8.0.0 LTS | Application runtime |
- | **Data Serialization** | System.Text.Json | .NET 8 built-in | JSON deserialization (no dependency) |
- | **Validation** | FluentValidation | 11.9.1 | Data schema validation on load |
- | **File I/O** | System.IO | .NET 8 built-in | Local snapshot management |
- | Component | Technology | Version | Purpose |
- |-----------|-----------|---------|---------|
- | **Current Data** | data.json (JSON file) | N/A | Single-source configuration file |
- | **Historical Snapshots** | File-based (weekly .json) | N/A | Trend data; append-only model |
- | **Schema** | Custom C# records | N/A | Strong-typed JSON deserialization |
- | Component | Technology | Version | Purpose |
- |-----------|-----------|---------|---------|
- | **PDF Generation** | QuestPDF | 2024.3.0 | Server-side executive report export |
- | Component | Technology | Version | Purpose |
- |-----------|-----------|---------|---------|
- | **Unit Testing** | xUnit | 2.6.6 | Test runner |
- | **Mocking** | Moq | 4.20.70 | Dependency mocking |
- | Component | Technology | Version | Purpose |
- |-----------|-----------|---------|---------|
- | **Server** | Kestrel (embedded) | .NET 8 built-in | HTTP server (localhost only) |
- | **Process Hosting** | Windows Service or Direct .exe | N/A | Local background service or manual launch |
- | **Self-contained Runtime** | .NET 8 SCD | 8.0.0 | Single-file deployment (no runtime install required) |
- | Component | Technology | Version | Purpose |
- |-----------|-----------|---------|---------|
- | **IDE** | Visual Studio 2022 | 17.9+ | C# development |
- | **Version Control** | Git | Any | Source control |
- | **Build System** | dotnet CLI | 8.0+ | Project compilation & publish |
- ---
- ```
- ┌─────────────────────────────────────────────┐
- │           Blazor Server Components          │
- │  (MilestoneTimeline, StatusBucket, etc.)    │
- └────────────────┬────────────────────────────┘
- │
- ┌────────────────▼────────────────────────────┐
- │        Blazor Page (Dashboard.razor)        │
- │  • Data binding via @inject services        │
- │  • Event handling (refresh, export)         │
- └────────────────┬────────────────────────────┘
- │
- ┌────────────────▼────────────────────────────┐
- │      Application Services (C#)              │
- │  • ProjectDataService (JSON loading)        │
- │  • SnapshotService (history management)     │
- │  • ExportService (PDF generation)           │
- └────────────────┬────────────────────────────┘
- │
- ┌────────────────▼────────────────────────────┐
- │     File System (Local Only)                │
- │  • data.json (current state)                │
- │  • data/snapshots/*.json (history)          │
- └─────────────────────────────────────────────┘
- ```
- **Startup:** `Program.cs` registers `IProjectDataService` as singleton; loads `data.json` into memory.
- **Request:** Blazor component injects `IProjectDataService`; calls `GetDataAsync()`.
- **Rendering:** Component receives `ProjectData`; binds to OxyPlot Gantt and Razor sub-components.
- **Refresh:** Executive clicks "Refresh" button; service reloads from disk (or watcher detects change).
- **Export:** `ExportService` uses QuestPDF to render current state as PDF; downloads to browser.
- **History:** `SnapshotService` runs scheduled task (e.g., weekly); appends dated JSON to `data/snapshots/`.
- ```
- Dashboard.razor (root page)
- ├── ExecutiveSummary.razor (KPI cards: metrics, velocity)
- ├── MilestoneTimeline.razor (OxyPlot Gantt chart)
- ├── WorkItemGrid.razor (three-column bucket layout)
- │   ├── StatusBucket.razor (Shipped)
- │   │   └── WorkItemCard.razor (repeating)
- │   ├── StatusBucket.razor (In-Progress)
- │   │   └── WorkItemCard.razor (repeating)
- │   └── StatusBucket.razor (Carried-Over)
- │       └── WorkItemCard.razor (repeating)
- └── ActionBar.razor (Refresh, Export PDF, Settings)
- ```
- ```csharp
- public record ProjectData(
- string ProjectName,
- string ReportingPeriod,
- List<Milestone> Milestones,
- List<WorkItem> WorkItems,
- ProjectMetrics Metrics
- );
- public record Milestone(
- string Id,
- string Name,
- DateTime DueDate,
- DateTime? CompletionDate,
- string Status, // "planned" | "in-progress" | "completed" | "blocked"
- string Priority // "critical" | "high" | "medium" | "low"
- );
- public record WorkItem(
- string Id,
- string Title,
- string Category, // "shipped" | "in-progress" | "carried-over"
- string Milestone,
- DateTime DueDate,
- DateTime? CompletionDate,
- decimal EstimatedHours,
- decimal? ActualHours,
- string Assignee
- );
- public record ProjectMetrics(
- decimal Velocity,
- decimal BurndownRate,
- List<string> Blockers
- );
- ```
- **Desktop (≥1024px):** 4-column grid for work-item buckets; full horizontal Gantt timeline.
- **Tablet (768px-1023px):** 2-column grid; compressed Gantt with horizontal scroll.
- **Mobile (<768px):** 1-column stacked layout; vertical milestone list instead of Gantt; work items shown as cards.
- Implemented via Tailwind CSS breakpoints and conditional component rendering based on viewport (server-side media query awareness via CSS).
- ---

### Considerations & Risks

- **Rationale:** "No auth, not enterprise security" per requirements. Dashboard is local-only, accessed from same LAN.
- **Future consideration:** If multi-user access required, add Windows Authentication (NTLM) or simple API key validation in `appsettings.json`.
- **In-transit:** Not applicable (local LAN only); HTTPS not required.
- **At-rest:** File permissions via OS (Windows folder ACL); no encryption necessary for local deployment.
- **JSON exposure:** Assume data.json contains no PII; executives may review raw file. If sensitive, encrypt with `System.Security.Cryptography.Aes`.
- **Publish:** `dotnet publish -c Release --self-contained --runtime win-x64 -p:PublishSingleFile=true`
- Produces single `.exe` (~120MB with .NET 8 runtime included)
- No .NET 8 installation required on executive machine
- Can be copied to USB or network share
- **Execution:** Double-click `.exe`; Kestrel binds to `http://localhost:5000` by default
- Browser auto-launches or user navigates to URL manually
- WebSocket connection established via localhost (no firewall issues)
- **Persistent Background:** Wrap `.exe` as Windows Service using `NSSM` (Non-Sucking Service Manager) or scheduled task
- Runs automatically on boot
- Accessible from multiple devices on LAN via `http://<machine-name>:5000`
- Deploy as IIS application for multi-device LAN access
- Requires .NET 8 hosting bundle on host machine
- Not recommended for single-user local deployment (adds operational complexity)
- **Local deployment:** $0 (use existing executive machine or old laptop)
- **Kestrel:** Free (built into .NET 8)
- **Tooling:** Visual Studio Community (free tier sufficient)
- **Backup:** Manual backup of `data.json` and `data/snapshots/` folder to USB/network (recommended weekly)
- **Logging:** Implement `ILogger` interface; write to `logs/` folder for debugging
- **Monitoring:** No monitoring required for local deployment; if needed, add Application Insights SDK (optional)
- ---
- | Risk | Severity | Mitigation |
- |------|----------|-----------|
- | **OxyPlot Gantt rendering complexity** | Medium | Prototype chart rendering in week 1; validate SVG export before committing |
- | **WebSocket connectivity on corporate LAN** | Low | Test with firewalls enabled; document port 5000 whitelisting if needed |
- | **File locking during snapshot writes** | Low | Use `FileShare.ReadWrite` when reading concurrent snapshot; unlikely in single-user scenario |
- | **JSON deserialization failures** | Medium | Add FluentValidation schema validation; log errors to file; graceful fallback to empty project |
- | **Historical snapshot bloat** | Low | Implement retention policy (keep last 52 weeks, delete older); typical cost <3MB |
- | **Blazor Server WebSocket disconnect** | Low | Automatic reconnection; browser F5 refresh restores connection |
- **Single Kestrel process:** Handles one concurrent executive user comfortably; not designed for high concurrency
- **In-memory cache:** Project data loaded entirely into RAM; typical 1000-item project = <10MB (negligible)
- **File I/O:** Reading/writing JSON is single-threaded; not a bottleneck for <1000 items
- | Decision | Pro | Con | Reasoning |
- |----------|-----|-----|-----------|
- | **Blazor Server vs. WebAssembly** | Full C#; server-side state | WebSocket dependency | Server-side rendering justified; local LAN eliminates latency |
- | **OxyPlot vs. Chart.js (Gantt)** | Pure C#; no JS | Less interactive | Gantt is table-stakes; interactive features unnecessary |
- | **File-based snapshots vs. SQLite** | Simple; auditable; no schema | File naming/versioning burden | Snapshots sufficient for trend analysis; avoid DB overhead |
- | **Startup cache vs. on-demand load** | Fast UI; predictable | Stale data if file changes | Startup acceptable; FileSystemWatcher alternative for auto-refresh |
- | **JSON vs. CSV/XML** | Human-readable; hierarchical | Larger file size | JSON is de facto standard; marginally larger size acceptable |
- | **Tailwind vs. Bootstrap** | Lightweight; utilities-first | Less pre-built components | Tailwind aligns with Blazor component model; CSS footprint minimal |
- | **Self-contained .exe vs. runtime-dependent** | Zero-install convenience | Larger file (120MB) | Convenience outweighs size for local deployment |
- ---
- **Snapshot cadence:** Weekly automatic snapshots sufficient, or require daily/on-demand? (Business decision: gather exec input on trend detail needs)
- **Milestone milestone thresholds:** What % completion triggers "at-risk" status in KPI cards? (Product: define SLAs with stakeholders)
- **Mobile-specific experience:** Prioritize responsive Gantt vs. separate mobile view? (UX: test with tablets before committing)
- **Data structure extensibility:** Will future phases require custom fields (e.g., budget, resource allocation)? (Architecture: design JSON schema for flexibility now)
- **Real-time collaboration:** Should multiple executives edit data.json simultaneously, or single-editor model? (Business: define governance model)
- **Historical data depth:** How far back should trend charts show (4 weeks, 12 weeks, 52 weeks)? (Product: balance detail vs. noise)
- **Export formats:** PDF sufficient, or require Excel/CSV exports? (Executive: gather preferred report formats)
- **Offline capability:** Can dashboard operate if data.json file is read-only or temporarily inaccessible? (Ops: define failure modes)
- ---

### Detailed Analysis

# Technology Stack Research: Executive Project Dashboard
## C# .NET 8 Blazor Server Implementation

---

## 1. Data Schema Architecture for Executive Dashboards

**Key Findings:**
The data.json structure must support hierarchical milestone tracking, categorical status aggregation, and temporal progress snapshots. Executive dashboards require historical context (carried-over items, trends) alongside current state.

**Recommended Schema:**
```json
{
  "projectName": "string",
  "reportingPeriod": "YYYY-MM",
  "milestones": [
    {
      "id": "string",
      "name": "string",
      "dueDate": "ISO8601",
      "completionDate": "ISO8601|null",
      "status": "planned|in-progress|completed|blocked",
      "priority": "critical|high|medium|low"
    }
  ],
  "workItems": [
    {
      "id": "string",
      "title": "string",
      "category": "shipped|in-progress|carried-over",
      "milestone": "string (reference)",
      "dueDate": "ISO8601",
      "completionDate": "ISO8601|null",
      "estimatedHours": "number",
      "actualHours": "number|null",
      "assignee": "string"
    }
  ],
  "metrics": {
    "velocity": "number (items/week)",
    "burndownRate": "number (%)",
    "blockers": ["string"]
  }
}
```

**Trade-offs & Alternatives:**
- **Nested vs. Flat:** Nested structure (milestones contain items) vs. flat with references. Chose flat with references for flexibility and easier JSON mutations.
- **Timestamp granularity:** ISO8601 sufficient for executive reporting; millisecond precision unnecessary.
- **Historical snapshots:** Consider appending date-stamped snapshots rather than versioning entire file.

**Concrete Recommendations:**
- Use `System.Text.Json` (built into .NET 8) for deserialization; no external dependency needed.
- Create a `ProjectData` C# record type matching schema for strong typing.
- Validate JSON on load using data annotations or FluentValidation v11.x.

**Evidence & Reasoning:**
Executives need milestone-centric views (timeline first) and work-item categorization (status bucketing). Flat structure enables filtering/grouping without recursive traversal. ISO8601 dates integrate seamlessly with .NET DateTime handling and UI formatting.

---

## 2. Blazor Server Charting & Timeline Components

**Key Findings:**
Blazor Server requires server-side component rendering; real-time interactivity via WebSocket. For timelines/charts, C# charting libraries with minimal JavaScript dependencies are preferred.

**Recommended Libraries:**

| Library | Version | Use Case | Trade-offs |
|---------|---------|----------|-----------|
| **OxyPlot** | 2.1.2 | Gantt charts, timelines | Pure C#, no JS dependency; mature but limited interactive features |
| **LiveCharts2** | 2.0.11 | Progress bars, KPI metrics | WPF/MAUI native; Blazor support via canvas; animated |
| **Chart.js via JS Interop** | 4.4.0 | Area/line trends | Industry standard; requires JS interop; excellent interactivity |
| **Plotly.NET** | 5.x (experimental Blazor) | Advanced analytics | Functional programming model; immature Blazor support |

**Concrete Recommendations:**
- **Primary:** OxyPlot 2.1.2 for Gantt/timeline visualization (critical for executive milestone view).
  - Provides `OxyPlot.Wpf.PlotView` serializable to SVG for Blazor rendering.
  - Use `TimeAxis` for milestone dates; `RectangleBarSeries` for work items.
  
- **Secondary:** Chart.js 4.4.0 via JS Interop for trend charts (velocity burndown).
  - Use `IJSRuntime` to invoke Chart.js from Blazor components.
  - Minimal wrapper: ~50 lines of C# code.

**Evidence & Reasoning:**
OxyPlot is stable, dependency-light, and specifically designed for server-side rendering. Chart.js via interop trades JS complexity for superior UX (animations, tooltips). Combination minimizes external .NET package debt while delivering rich executive visuals.

---

## 3. Blazor Component Refactoring from HTML Template

**Key Findings:**
OriginalDesignConcept.html must be deconstructed into reusable, data-driven Blazor components. The challenge: preserve visual hierarchy and executive aesthetic while enabling dynamic data binding.

**Recommended Component Structure:**

```
Dashboard.razor (root)
├── MilestoneTimeline.razor
├── WorkItemGrid.razor
│   ├── StatusBucket.razor
│   └── WorkItemCard.razor
├── MetricsPanel.razor
├── ExecutiveSummary.razor
└── ExportButton.razor
```

**Concrete Recommendations:**
1. **MilestoneTimeline.razor:** Renders OxyPlot Gantt chart; accepts `List<Milestone>` as parameter.
2. **StatusBucket.razor:** Groups work items by category (shipped/in-progress/carried-over). Use CSS Grid for responsive layout.
3. **MetricsPanel.razor:** KPI badges (velocity, burn rate, blockers). Isolated data binding prevents re-renders.
4. **Export to PDF:** Use QuestPDF v2024.3 (latest stable .NET 8 version) for server-side PDF generation; no JavaScript PDF library needed.

**Trade-offs & Alternatives:**
- **Razor Components vs. Blazor Interactive (Auto):** Chose Server-side for simplicity; no JavaScript framework overhead.
- **CSS Framework:** Tailwind CSS v3 (minimal footprint) vs. Bootstrap 5 (heavier). Recommend Tailwind for lightweight executive aesthetic.

**Evidence & Reasoning:**
Component isolation ensures maintainability. Avoiding JavaScript frameworks aligns with "local-only, no enterprise complexity" constraint. OxyPlot + Razor components = pure C# pipeline.

---

## 4. Local File Data Loading in Blazor Server

**Key Findings:**
Blazor Server executes server-side; file I/O is direct OS-level operation. For local data.json reading, optimal pattern: load at app startup into memory, refresh on-demand.

**Concrete Recommendations:**

**Pattern 1: Startup Load (Recommended)**
```csharp
// Program.cs
builder.Services.AddSingleton<IProjectDataService, ProjectDataService>();

// ProjectDataService.cs
public class ProjectDataService
{
    private readonly string _dataPath = "data.json";
    private ProjectData _cache;

    public async Task<ProjectData> GetDataAsync()
    {
        if (_cache == null)
        {
            var json = await File.ReadAllTextAsync(_dataPath);
            _cache = JsonSerializer.Deserialize<ProjectData>(json);
        }
        return _cache;
    }

    public async Task RefreshAsync()
    {
        _cache = null;
        await GetDataAsync();
    }
}
```

**Pattern 2: File Watcher (Advanced)**
Use `System.IO.FileSystemWatcher` to auto-refresh on data.json changes.

**Trade-offs:**
- **Startup load:** Fast UI; stale data if file updated. ✓ Recommended for local/simple use.
- **On-demand reload:** Always fresh; disk I/O per request. Acceptable for local use.
- **FileSystemWatcher:** Realtime updates; requires background thread. Use for multi-user scenarios.

**Concrete Implementation:**
- Use `System.Text.Json` (built-in .NET 8); no dependency.
- Add `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for JSON flexibility.
- Implement `IAsyncNotifyPropertyChanged` for Blazor component binding.

**Evidence & Reasoning:**
Local file I/O has no latency concerns; in-memory caching eliminates disk access per render. Startup load balances simplicity vs. responsiveness.

---

## 5. Charting Library Selection: Comparative Deep Dive

**Key Findings:**
Executive dashboard priorities: **clarity > interactivity**, **performance > features**, **maintenance < complexity**.

**Detailed Comparison:**

| Criterion | OxyPlot 2.1.2 | Chart.js 4.4 | LiveCharts2 2.0.11 | Plotly.NET 5.x |
|-----------|---------------|--------------|-------------------|----------------|
| **Blazor Support** | SVG export | JS Interop | Canvas Interop | Experimental |
| **Gantt Charts** | Native ✓ | No (custom) | No | No |
| **Learning Curve** | Moderate | Low | Moderate | High (F#) |
| **Performance** | Excellent | Good | Good | Unknown |
| **Maintenance Burden** | Low | Medium (JS) | Low | High |
| **.NET 8 Compatibility** | Full | Via Interop | Full | Partial |
| **Community Size** | Small | Large (JS) | Growing | Tiny |
| **License** | MIT | MIT | MIT | MIT |

**Recommendation by Chart Type:**
- **Gantt/Timeline:** OxyPlot exclusively. No competitor exists in .NET.
- **Line/Area (Trends):** Chart.js (via interop) for polish; OxyPlot for simplicity.
- **Progress Bars/KPIs:** CSS + Blazor components (no library needed).

**Concrete Package Versions:**
- `OxyPlot.Core` 2.1.2
- `Chart.js` 4.4.0 (via CDN in `index.html`, not NuGet)
- Skip LiveCharts2 for this project; overkill for dashboard use case.

**Evidence & Reasoning:**
Gantt charts are non-negotiable for executive milestone view; OxyPlot is the only production-ready option in .NET ecosystem. Chart.js via interop adds minimal complexity while delivering superior UX for trend visualization. Avoid experimental/immature libraries in executive-facing tools.

---

## 6. Responsive Design & Multi-Device Support

**Key Findings:**
Executives access dashboards from desktop (detailed analysis), tablet (status checks), mobile (on-the-go visibility). Responsive design is critical; Blazor Server handles this server-side with CSS media queries.

**Concrete Recommendations:**

**CSS Framework:** Tailwind CSS v3.4.1
- Lightweight: 15KB minified (vs. Bootstrap 5: 160KB)
- Utilities-first aligns with component-driven Blazor
- No JavaScript dependencies

**Layout Strategy:**
```html
<!-- Dashboard responsive grid -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
  <!-- Executive Summary Cards -->
</div>

<!-- Timeline adapts: -->
<!-- Desktop: Full horizontal Gantt -->
<!-- Tablet: Compressed Gantt + scroll -->
<!-- Mobile: Vertical milestone list -->
```

**Viewport Meta Tag (Critical):**
```html
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
```

**Chart Responsiveness:**
- OxyPlot: Use `PlotView` with `Width="100%"` and CSS containment.
- Chart.js: Set `responsive: true, maintainAspectRatio: false` in config.

**Trade-offs:**
- **Tailwind vs. Bootstrap:** Tailwind lighter but less pre-built components. Recommend Tailwind for this use case.
- **Fixed vs. Fluid layouts:** Fluid (CSS Grid) preferred; accommodates all devices.

**Evidence & Reasoning:**
Executives increasingly use mobile for quick status checks. Server-side CSS media queries avoid JavaScript overhead. Tailwind minimizes bundle size while providing responsive utilities natively.

---

## 7. Local Data Persistence & Historical Snapshots

**Key Findings:**
Executive dashboards must show trends (velocity over weeks, milestone slip). "Carry-over" items require historical context. Local storage strategy must balance simplicity vs. historical accuracy.

**Recommended Approach: File-Based Snapshots**

**Pattern:**
```
/data/
├── data.json (current)
└── snapshots/
    ├── 2026-04-01.json
    ├── 2026-03-25.json
    └── 2026-03-18.json
```

**Implementation:**
```csharp
public class SnapshotService
{
    public async Task CreateSnapshotAsync(ProjectData data)
    {
        var filename = $"{DateTime.Now:yyyy-MM-dd}.json";
        var path = Path.Combine("data/snapshots", filename);
        var json = JsonSerializer.Serialize(data);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<List<ProjectData>> GetHistoryAsync()
    {
        var files = Directory.GetFiles("data/snapshots", "*.json")
            .OrderByDescending(f => f)
            .Take(12) // Last 12 weeks
            .Select(async f => JsonSerializer.Deserialize<ProjectData>(
                await File.ReadAllTextAsync(f)))
            .Select(t => t.Result)
            .ToList();
        return files;
    }
}
```

**Trade-offs & Alternatives:**
- **SQLite:** Overkill for local, simple project. Adds dependency/maintenance.
- **File-based snapshots:** Simple, auditable, human-readable. ✓ Recommended.
- **CSV append:** Too unstructured for complex hierarchical data.

**Storage Overhead:**
Each snapshot ~10-50KB (typical project). 52-week history = 520KB-2.6MB. Negligible for local deployments.

**Evidence & Reasoning:**
File-based snapshots leverage .NET's file I/O without external DB. JSON is auditable (exec can inspect raw data). Weekly snapshots sufficient for trend analysis. No schema migration complexity.

---

## 8. Architecture Decision: Blazor Server vs. Static HTML + Interop

**Key Findings:**
Two viable approaches exist:
1. **Blazor Server:** Full C# on server; real-time WebSocket updates.
2. **Static HTML + JavaScript:** Simpler hosting; stateless frontend.

Given project constraints (local-only, simple, no enterprise complexity), this decision is critical.

**Comparative Analysis:**

| Factor | Blazor Server | Static HTML + JS |
|--------|--------------|------------------|
| **Deployment** | IIS / Kestrel (server required) | Any static host / browser | ✓ Advantage: Static |
| **Development Speed** | Fast (full C#) | Slower (C# + JS bridge) | ✓ Advantage: Blazor |
| **Runtime Dependencies** | .NET 8 runtime | None (browser) | ✓ Advantage: Static |
| **Real-time Updates** | WebSocket native ✓ | Polling/fetch | ✓ Advantage: Blazor |
| **Learning Curve** | C# developer friendly ✓ | C# + JavaScript split | ✓ Advantage: Blazor |
| **Scalability** | StatefulServer limits | Stateless scales | ✓ Advantage: Static |
| **Local Deployment** | Simple (single EXE) ✓ | Simple (copy HTML) | ≈ Tie |

**Concrete Recommendation: Blazor Server**

**Reasoning:**
1. **Team skill fit:** "C# .NET 8" mandate implies C# expertise; Blazor leverages this fully.
2. **Development velocity:** Full C# stack eliminates JS context-switching.
3. **Feature parity:** Blazor Server handles real-time updates, component state management natively.
4. **Local deployment:** Single executable via `dotnet publish`; minimal operational overhead.
5. **Enterprise-lite approach:** Blazor Server offers structure without heavyweight frameworks.

**Implementation Strategy:**
- Use Blazor Server (not WebAssembly) for server-side rendering + interactivity.
- Deploy as self-contained .NET 8 app: `dotnet publish -c Release --self-contained`.
- Target Windows for local desktop: `--rid win-x64`.
- Result: Single .exe file; no .NET 8 runtime installation required on exec machine.

**Trade-offs Addressed:**
- **WebSocket overhead:** Acceptable for local LAN; no internet latency.
- **Stateful server limitation:** Not a concern for single local instance.
- **Complexity:** Blazor Server is simpler than HTML + JS interop bridge.

**Evidence & Reasoning:**
For "local-only, simple, no enterprise" + ".NET 8 mandate," Blazor Server is optimal. It eliminates JS entirely, reduces deployment surface, and delivers rich interactivity without added complexity. Static HTML would require JavaScript spike and deployment complexity (separate host + frontend bundle).

---

## Implementation Priorities & Risks

**Quick Wins (Week 1):**
1. Load data.json; display static milestone list.
2. Build StatusBucket components with sample data.
3. Integrate OxyPlot for Gantt chart rendering.

**Risks:**
1. **OxyPlot learning curve:** Research SVG export mechanics early.
2. **WebSocket connectivity:** Local LAN may have firewall rules; test early.
3. **PDF export (QuestPDF):** Test rendering before committing to pipeline.

**Deferred Decisions:**
- Historical snapshot frequency (weekly vs. daily) — decide after MVP demo.
- Mobile-specific UX (separate mobile view vs. responsive) — gather exec feedback.
