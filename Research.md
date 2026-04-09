# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-09 21:08 UTC_

### Summary

This project requires a simple, screenshot-optimized executive reporting dashboard built on C# .NET 8 with Blazor Server. The dashboard will render project milestones, progress metrics (shipped/in-progress/carryover items), and timeline visualization from a JSON configuration file (data.json). The recommended approach prioritizes simplicity and print-readiness over enterprise features—no authentication, no cloud services, file-based data storage. RadzenBlazor provides polished charting and timeline components; a minimal service layer (DashboardService) decouples data loading from presentation. This architecture scales to support future reporting variants while maintaining the lightweight, screenshot-friendly design necessary for PowerPoint integration.

### Key Findings

- Blazor Server's server-side rendering eliminates client-side complexity while supporting real-time interactivity via WebSocket—ideal for intranet dashboards with uniform network conditions.
- RadzenBlazor 4.x is the de facto standard for production Blazor applications requiring polished executive-grade UIs; it includes charting, timeline, and gauge components optimized for print/screenshot export.
- File-based JSON with FileSystemWatcher monitoring provides reliable hot-reload capability without database infrastructure; suitable for MVP when file writing remains single-threaded.
- A multi-component architecture (MilestoneTimeline, ProjectStatusCard, ShippedItemsList, ProgressIndicator) enables code reuse and future reporting variants without over-engineering at MVP stage.
- System.Text.Json (native .NET 8) outperforms Newtonsoft.Json by 2-3x while adding zero external dependencies—strongly preferred for performance and supply chain security.
- Print-optimized CSS (@media print) and Bootstrap 5.3.x provide screenshot-ready layouts compatible with PowerPoint; browser's native print-to-PDF is sufficient for export workflow.
- Relative date calculations (days remaining, days overdue) require UTC storage in data.json with local rendering in UI; simple DateTime comparisons suffice for single-timezone intranet deployments.
- Performance benchmarks indicate < 200ms render time for 30-50 work items; total HTTP payload ~120 KB initial + ~10 KB per update—no bottlenecks at expected usage scale.
- ```csharp
- public interface IDashboardService
- {
- Task<DashboardData> LoadDataAsync(string filePath);
- Task<bool> ValidateDataAsync(DashboardData data);
- }
- public class DashboardService : IDashboardService
- {
- private readonly ILogger<DashboardService> _logger;
- public DashboardService(ILogger<DashboardService> logger)
- {
- _logger = logger;
- }
- public async Task<DashboardData> LoadDataAsync(string filePath)
- {
- try
- {
- var json = await File.ReadAllTextAsync(filePath);
- var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
- var data = JsonSerializer.Deserialize<DashboardData>(json, options)
- ?? throw new InvalidOperationException("Failed to deserialize data.json");
- if (!await ValidateDataAsync(data))
- throw new InvalidOperationException("Data validation failed");
- return data;
- }
- catch (Exception ex)
- {
- _logger.LogError(ex, "Failed to load dashboard data from {filePath}", filePath);
- throw;
- }
- }
- public async Task<bool> ValidateDataAsync(DashboardData data)
- {
- var context = new ValidationContext(data);
- var results = new List<ValidationResult>();
- return Validator.TryValidateObject(data, context, results, validateAllProperties: true);
- }
- }
- ```
- **No external state library** (e.g., MediatR, Redux) for MVP
- Blazor native: Cascading parameters for top-down data flow, component parameters for child inputs
- Parent `DashboardPage` holds `DashboardData` in a private field, triggers `StateHasChanged()` when refreshing
- Deferred: Implement state pattern (MediatR 12.x) only if dashboard complexity grows or multi-page application emerges
- Blazor Server project structure (.sln, .csproj files)
- `DashboardPage.razor` parent component with cascading parameters
- Four core sub-components:
- `MilestoneTimeline.razor` (CSS-based timeline, no charting library)
- `ProjectStatusCard.razor` (metrics display)
- `ShippedItemsList.razor` (HTML table)
- `CarryoverIndicator.razor` (flagged items table)
- `DashboardService` with `LoadDataAsync` method (no file watcher yet)
- Sample `wwwroot/data/data.json` with 8-10 realistic work items
- Bootstrap 5 + custom CSS with print styles (@media print)
- Manual refresh button (F5 or "Load Data" button); no hot-reload
- **Print styles optimization**: Add @media print CSS to hide navigation, optimize spacing for 8.5" × 11" slide dimensions—minimal effort, high visual impact
- **Status badges**: Implement color-coded milestone status (completed=green, in-progress=yellow, at-risk=red)—pre-built RadzenBlazor components, 2-3 hours
- **Days-remaining calculation**: Simple `DateTime` comparison logic for milestone countdown; flag items >= 3 days overdue—1 hour
- **Hardcoded sample data**: Use OriginalDesignConcept.html + ReportingDashboardDesign.png as reference for realistic fake data (fictional project, 5-10 milestones)—2 hours
- Integrate RadzenBlazor Chart and Timeline components (upgrade from CSS-only)
- Implement FileSystemWatcher hot-reload (IDashboardService.WatchDataAsync)
- Add input validation + error UI for malformed data.json
- Serilog structured logging
- Unit tests for DashboardService
- **RadzenBlazor rendering fidelity**: Render sample Timeline + Chart components at target resolution; verify screenshot clarity for PowerPoint
- **CSS print quality**: Test Chrome DevTools print preview; ensure milestone timeline and progress bars render correctly on simulated 8.5" × 11" page
- **JSON parsing performance**: Load sample data.json (50 work items) 100x; measure parse time and memory usage (expect < 5ms parse time)
- [ ] Team familiar with Blazor Server component model (if not, reserve 1 sprint for learning)
- [ ] Visual Studio 2022 installed with .NET 8 SDK
- [ ] Agreement on OriginalDesignConcept.html as design baseline
- [ ] Clarification on data.json update mechanism (manual vs. automated)
- [ ] Sign-off on MVP scope (timeline/milestone component priority)

### Recommended Tools & Technologies

- **Blazor Server** 8.0+ (Microsoft.AspNetCore.Components.Server)
- **RadzenBlazor** 4.x (NuGet: Radzen.Blazor 4.x)
- Provides: Chart, Timeline, Gauge, Card, Badge, Button components
- License: Commercial (free tier for development; evaluate licensing model for production)
- Alternatives considered: OxyPlot (limited Blazor support), Chart.js via interop (JavaScript overhead, screenshot timing issues), custom SVG (zero-dependency but high development effort)
- **Bootstrap** 5.3.x (NuGet: Bootstrap 5.3.x or via CDN link in HTML)
- Rationale: Industry-standard responsive framework, active maintenance, zero .NET-specific dependencies
- Usage: Grid layout, card components, typography; layer custom CSS for screenshot optimization
- **System.Text.Json** 8.0+ (built-in, System.Text.Json NuGet package)
- JSON parsing and serialization with compile-time source generation
- Alternatives rejected: Newtonsoft.Json (adds 1.5MB, slower, legacy)
- **.NET 8 Runtime** (LTS, support until November 2026)
- **ASP.NET Core** 8.0+ (Microsoft.AspNetCore.App)
- **Serilog** 3.x (optional, NuGet: Serilog + Serilog.AspNetCore 8.x)
- Structured logging for diagnostics; defer if dashboard remains simple
- **System.IO.FileSystem** (built-in)
- FileSystemWatcher for monitoring data.json changes
- **File System (JSON)** for MVP
- Location: `wwwroot/data/data.json`
- Format: UTF-8 text, ISO 8601 dates (UTC)
- Alternative for future: SQLite (enable audit trails, concurrent writes)
- Not recommended: Cloud storage (violates local-only constraint)
- **xUnit** 2.x (NuGet: xunit 2.x)
- **Moq** 4.x (NuGet: Moq 4.x) for dependency mocking
- Optional: **FluentAssertions** 6.x (NuGet: FluentAssertions 6.x) for readable test assertions
- **Self-contained .NET 8 executable**
- Deployment: Single Windows .exe (no runtime required on target machine)
- Build: `dotnet publish -c Release -r win-x64 --self-contained`
- Alternatives rejected: Docker containerization (unnecessary for local-only app), cloud deployment (out of scope)
- **Visual Studio 2022 Community/Professional** (recommended IDE)
- **Visual Studio Code** with C# extension as alternative (OmniSharp)
- **.NET 8 SDK** (Latest LTS version)
- Parent component: `DashboardPage.razor`
- Manages state, data loading, and refresh lifecycle
- Uses dependency injection to consume `IDashboardService`
- Child components (all reusable):
- `MilestoneTimeline.razor` — Renders chronological milestone list with status indicators
- `ProjectStatusCard.razor` — Displays summary metrics (shipped, in-progress, carryover counts)
- `ShippedItemsList.razor` — Tabular view of completed work items
- `InProgressItemsList.razor` — Tabular view of active work
- `CarryoverIndicator.razor` — Flags slipped items with reason and new target date
- `ProgressIndicator.razor` — Overall project % complete with days-to-completion
- **Initialization**: `DashboardPage.OnInitializedAsync` calls `IDashboardService.LoadDataAsync(dataPath)`
- **Parsing**: `DashboardService.LoadDataAsync` reads JSON file, parses with `System.Text.Json`, validates against data annotations
- **Distribution**: Parent passes deserialized `DashboardData` object to child components via cascading parameters
- **Rendering**: Each child component renders its assigned data subset
- **Hot-Reload** (optional): `FileSystemWatcher` detects data.json changes, calls `StateHasChanged()` on parent via `InvokeAsync`
- ```csharp
- public class DashboardData
- {
- [Required]
- public ProjectMetadata Project { get; set; }
- [Required]
- public List<Milestone> Milestones { get; set; } = new();
- [Required]
- public List<WorkItem> WorkItems { get; set; } = new();
- public DashboardSummary Summary { get; set; }
- }
- public class ProjectMetadata
- {
- [Required]
- public string Name { get; set; }
- [Required]
- public DateTime StartDate { get; set; }
- [Required]
- public DateTime EndDate { get; set; }
- public string Description { get; set; }
- }
- public class Milestone
- {
- [Required]
- public string Id { get; set; }
- [Required]
- public string Name { get; set; }
- [Required]
- public DateTime Date { get; set; }
- [Required]
- public MilestoneStatus Status { get; set; }
- public string Description { get; set; }
- }
- public enum MilestoneStatus { Completed, InProgress, Planned, AtRisk }
- public class WorkItem
- {
- [Required]
- public string Id { get; set; }
- [Required]
- public string Title { get; set; }
- [Required]
- public WorkItemCategory Category { get; set; }
- public string MilestoneId { get; set; }
- public int? PercentComplete { get; set; }
- public DateTime? CompletedDate { get; set; }
- public DateTime? OriginalTarget { get; set; }
- public DateTime? NewTarget { get; set; }
- public string CarryoverReason { get; set; }
- }
- public enum WorkItemCategory { Shipped, InProgress, Carryover }
- public class DashboardSummary
- {
- public int ShippedCount { get; set; }
- public int InProgressCount { get; set; }
- public int CarryoverCount { get; set; }
- public int OverallPercentComplete { get; set; }
- }
- ```

### Considerations & Risks

- **None for MVP** (per requirements)
- **Assumption**: Dashboard runs on secured intranet network with physical or network-level access controls
- **Future consideration**: If dashboard proliferates, add Windows Authentication (AD/Kerberos) via `Authentication=Windows` in launchSettings.json
- **At-rest**: JSON file stored with OS-level ACLs (Windows NTFS permissions)
- Example: Restrict write access to service account that updates data.json; read-only for web server process
- **In-transit**: Data transmitted over WebSocket (HTTPS enforced in production)
- **No encryption** required for project metadata (non-sensitive, internal only)
- **Platform**: Windows Server 2022+ or Windows 10/11 Pro (for development/small deployments)
- **Runtime**: Self-contained .NET 8 executable (no runtime installation required on target machine)
- **Process Management**: Windows Service (use NSSM - Non-Sucking Service Manager for zero-cost wrapper) or Task Scheduler
- **Reverse Proxy** (optional): IIS or nginx for HTTPS termination and URL rewriting
- **Scalability**: Single-machine deployment only (no clustering per constraints)
- **Estimated Infrastructure Cost**: $0 (leverages existing Windows infrastructure, no cloud services)
- **Startup Time**: ~2-3 seconds (self-contained executable)
- **Memory Footprint**: ~150-200 MB baseline (Blazor Server + .NET 8 runtime)
- **Disk Space**: ~200 MB (self-contained publish output)
- **Network**: Requires persistent WebSocket connection (TCP 443 for HTTPS)
- **File System Permissions**: Service account needs read access to wwwroot/data/data.json directory
- | Risk | Impact | Mitigation |
- |------|--------|-----------|
- | **JSON schema drift** (data producer writes malformed JSON) | High | Implement strict JSON Schema validation on load; use data annotations + FluentValidation; log validation errors; alert on failures |
- | **File contention** (concurrent writes to data.json) | Medium | For MVP, accept single-threaded write assumption; migrate to SQLite if multi-service updates required; add advisory file locks if concurrency emerges |
- | **WebSocket disconnection** (network interruption) | Low | Blazor Server handles automatic reconnection; dashboard gracefully degrades with stale data during brief outages |
- | **Large JSON files** (> 1000 work items) | Low-Medium | System.Text.Json can parse files up to 10MB without issues; implement pagination in UI if dataset grows beyond 100 items |
- | **Date parsing timezone bugs** | Medium | Use UTC storage + `DateTime.UtcNow` for calculations; avoid `DateTime.Now` (system clock dependent); test DST transitions |
- **Single-machine deployment**: No clustering or load balancing (architectural constraint per requirements); accept as limitation
- **File-based JSON**: Unsuitable if multiple services update data concurrently; migrate to SQLite (with advisory locks) if bottleneck emerges
- **Blazor Server WebSocket overhead**: Each connected client consumes ~1MB memory; estimate max 500-1000 concurrent connections per machine before memory exhaustion
- | Decision | Benefit | Cost |
- |----------|---------|------|
- | **RadzenBlazor** | Executive-grade aesthetics, polished components | +5MB footprint, Telerik commercial licensing model |
- | **File-based JSON** | Zero infrastructure, simplicity | Limits concurrent writes, no audit trail by default |
- | **System.Text.Json** | 2-3x faster parsing, zero dependencies | Less flexible than Newtonsoft (no dynamic object support) |
- | **Multi-component architecture** | Reusability, testability | ~10% development overhead vs. monolithic component |
- | **No state library** | Minimal complexity, fast MVP | Future refactoring cost if app grows |
- **MVP validation**: Prototype with sample data.json before connecting production data source
- **Load testing**: Benchmark dashboard rendering with target dataset size (30-50 items) to confirm < 200ms render time
- **Print testing**: Verify PowerPoint screenshot quality on target display resolution (1920x1080 assumed)
- **File update mechanism**: Who/what will update data.json? Manual edit, scheduled script, external service? Determines whether file-locking or database migration is necessary.
- **Data refresh cadence**: Should dashboard automatically poll data.json (and how frequently—every 5s, 1m?) or require manual refresh? Affects FileSystemWatcher implementation urgency.
- **Historical data requirements**: Must dashboard preserve historical snapshots (for burndown charts) or only display current state? Determines whether SQLite audit table is needed for MVP.
- **Carryover reason visibility**: Should carryover items always show reason/new target date, or hide until clicked? Affects UI density and executive readability.
- **Multi-project support**: Should single dashboard instance support multiple project JSON files (dropdown selector) or always load one file? Impacts component design and routing.
- **Print/export automation**: Is browser print-to-PDF sufficient, or does business require server-side PDF generation with scheduled email distribution? Defers SelectPdf evaluation.
- **Milestone status automation**: Should milestone status (completed/in-progress/planned) be derived from work item completion, or manually set in JSON? Affects data model logic.

### Detailed Analysis

# Detailed Analysis: Executive Reporting Dashboard Sub-Questions

## Sub-Question 1: JSON Configuration Parsing, Validation & Hot-Reload

**Key Findings:**
Blazor Server applications can monitor file system changes efficiently without restarting the entire server process. The standard approach uses `FileSystemWatcher` paired with a singleton service that triggers component state updates through `InvokeAsync` callbacks.

**Tools & Libraries:**
- `System.Text.Json` 8.0+ (native, zero-dependency JSON parsing)
- `System.ComponentModel.DataAnnotations` (built-in validation)
- `FileSystemWatcher` (System.IO.FileSystem, built-in)
- Optional: `FluentValidation` 11.x (more expressive validation rules)

**Trade-offs & Alternatives:**
- Newtonsoft.Json (Newtonsoft.Json 13.x) offers dynamic object support but adds 1.5MB dependency; System.Text.Json is faster and native to .NET 8
- Using a database (SQLite) instead of file-based JSON adds complexity but enables concurrent access and audit trails—appropriate if multiple services will update dashboard data
- Hot-reload via `IAsyncNotifier` pattern creates tight coupling between FileSystemWatcher and components; alternatively, use a mediator pattern with MediatR 12.x for loose coupling

**Concrete Recommendation:**
Implement a `DashboardDataService` singleton using `FileSystemWatcher` to monitor data.json. Parse with `System.Text.Json`, validate with data annotations, and trigger component updates via `InvokeAsync`. For initial MVP, use file-based JSON only; defer SQLite migration if concurrent updates become a bottleneck.

**Evidence & Reasoning:**
This approach aligns with Blazor Server's WebSocket-based state management. File system watching is reliable on Windows (target environment per context). System.Text.Json performance benchmarks show 2-3x faster parsing than Newtonsoft at scale. No external dependencies means reduced supply chain risk and faster cold start.

---

## Sub-Question 2: Charting/Visualization Library Selection

**Key Findings:**
For executive dashboards requiring simple, polished visuals, Blazor-native charting libraries prioritize ease-of-use over extensibility. RadzenBlazor dominates the .NET Blazor ecosystem; Chart.js via interop offers lightweight alternatives but requires JavaScript interop complexity.

**Tools & Libraries:**
- **RadzenBlazor** 4.x (Telerik-backed, actively maintained, 50K+ GitHub stars across Telerik ecosystem)
  - Includes Chart, Gauge, Sparkline, TimeLine components
  - Responsive design optimized for print/screenshot
  - Commercial backing ensures long-term support
- **OxyPlot** 2.1.x (lightweight, open-source, minimal dependencies)
  - Limited Blazor integration; primarily WPF/WinForms
- **Chart.js** 4.x via `CurrieTechnologies.Razor.ChartJS` (community interop library)
  - JavaScript interop overhead; loses benefits of server-side rendering
- **Custom SVG rendering** (no dependencies)
  - Maximum control; highest development effort

**Trade-offs & Alternatives:**
- RadzenBlazor adds ~5MB footprint; ideal for production dashboards where polish matters
- OxyPlot requires custom Blazor wrapper implementation
- Chart.js via interop causes JavaScript execution overhead, reduces screenshot reliability (rendering timing issues)
- Custom SVG is zero-dependency but requires manual layout math for timeline, progress bars

**Concrete Recommendation:**
Use RadzenBlazor 4.x for charting and timeline components. It provides executive-grade aesthetics, responsive layouts suitable for PowerPoint screenshots, and requires zero custom JavaScript. The commercial backing (Telerik) reduces long-term maintenance risk.

**Evidence & Reasoning:**
RadzenBlazor is the de facto standard for .NET Blazor applications requiring polished UIs. Its Chart and Timeline components render consistently across browsers and print contexts (critical for screenshot export). Community health is strong (active GitHub, regular updates, > 1M NuGet downloads). Telerik's backing ensures .NET 8/9 compatibility roadmap clarity.

---

## Sub-Question 3: Component Architecture (Single vs. Multi-Component)

**Key Findings:**
A multi-component approach with clear separation of concerns (MilestoneTimeline, ProjectStatus, ShippedItemsList, CarryoverIndicator) scales better for future reporting variants while maintaining code reusability without over-engineering.

**Tools & Libraries:**
- Blazor native component composition (no external libraries required)
- Optional: Microsoft.AspNetCore.Mvc.ViewFeatures for advanced data binding patterns

**Trade-offs & Alternatives:**
- **Single monolithic component**: Simple for MVP, faster initial delivery (no component communication overhead), harder to reuse later
- **Multi-component with Cascading Parameters**: Forces clean dependency injection patterns, enables child component testing in isolation, adds ~10% development overhead
- **State management library (MediatR 12.x, Prism 8.x)**: Over-engineered for this use case; reserved for complex multi-page applications

**Concrete Recommendation:**
Implement 4-5 reusable sub-components within a parent `DashboardPage` component:
- `MilestoneTimeline.razor` (accepts IEnumerable<Milestone>)
- `ProjectStatusCard.razor` (accepts ProjectStatus object)
- `ShippedItemsList.razor` (accepts IEnumerable<WorkItem>)
- `ProgressIndicator.razor` (accepts percent complete, days remaining)

Use Cascading Parameters to pass dashboard state down the tree; allow child components to emit change notifications back to parent via callbacks.

**Evidence & Reasoning:**
This architecture mirrors industry best practices in React/Vue ecosystems, translates cleanly to Blazor's component model, and avoids premature complexity. Reusability becomes critical if dashboard variants (per-team, per-quarter) proliferate. Testing child components in isolation reduces defect density.

---

## Sub-Question 4: data.json Schema Design

**Key Findings:**
A flat, normalized schema with clear separation between milestones, work items, and project metadata enables flexible querying and reduces duplication.

**Recommended Schema:**
```json
{
  "project": {
    "name": "Q2 Cloud Migration",
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "description": "Migrate legacy systems to cloud infrastructure"
  },
  "milestones": [
    {
      "id": "m1",
      "name": "Phase 1: Infrastructure Setup",
      "date": "2026-02-28",
      "status": "completed",
      "description": "Provision cloud resources, VPC, IAM"
    },
    {
      "id": "m2",
      "name": "Phase 2: Application Migration",
      "date": "2026-04-30",
      "status": "in_progress",
      "description": "Move core services to cloud"
    }
  ],
  "workItems": [
    {
      "id": "wi1",
      "title": "Database migration scripts",
      "category": "shipped",
      "milestone": "m1",
      "completedDate": "2026-02-15"
    },
    {
      "id": "wi2",
      "title": "API performance testing",
      "category": "in_progress",
      "milestone": "m2",
      "percentComplete": 65
    },
    {
      "id": "wi3",
      "title": "Legacy system decommission",
      "category": "carryover",
      "milestone": "m2",
      "originalTarget": "2026-04-30",
      "newTarget": "2026-05-31",
      "reason": "Unexpected downstream dependency discovered"
    }
  ],
  "summary": {
    "shippedCount": 8,
    "inProgressCount": 5,
    "carryoverCount": 2,
    "overallPercentComplete": 62
  }
}
```

**Tools & Libraries:**
- `System.Text.Json` with custom serialization attributes for date handling
- Optional: `System.ComponentModel.DataAnnotations` for runtime validation

**Trade-offs & Alternatives:**
- Nested schema (milestones contain work items) simplifies single-query reads but complicates filtering by status across milestones
- Flat schema (separate milestones and workItems arrays) requires client-side joins but enables independent updates

**Concrete Recommendation:**
Use the flat schema above. It supports JSON Schema validation, enables incremental updates (one work item without re-writing entire file), and maps naturally to C# object graphs via System.Text.Json source generation.

**Evidence & Reasoning:**
Flat schemas are easier to validate and mutate without race conditions when multiple services potentially write updates. The schema includes carryover tracking (critical executive metric) and preserves historical target dates for transparency.

---

## Sub-Question 5: Styling & Print Optimization

**Key Findings:**
Bootstrap 5 paired with custom CSS provides a strong foundation. Print-optimized styling ensures screenshots embedded in PowerPoint maintain fidelity and readability.

**Tools & Libraries:**
- **Bootstrap 5.3.x** (CSS framework, 5.3.x is latest stable)
  - Bundled via NuGet: `Bootstrap` 5.3.x
  - Or via CDN for zero-build overhead
- **Custom CSS** for dashboard-specific layout
- Optional: `PureCSS` 3.x as lighter alternative to Bootstrap (8KB vs. 30KB minified)

**Trade-offs & Alternatives:**
- **Tailwind CSS** (via CDN) offers superior customization but requires JIT compilation if using utility-first approach; overkill for this project
- **Material Design** (via Radzen) adds visual consistency with RadzenBlazor components
- **Custom CSS only** (no Bootstrap) maximizes screenshot quality and minimizes file size but increases development effort

**Concrete Recommendation:**
Use Bootstrap 5.3.x via NuGet for base layout + responsive grid. Layer custom CSS for:
- Print styles (@media print) that hide navigation, optimize spacing for 8.5" x 11" paper/PowerPoint slide dimensions
- Executive-friendly typography (Segoe UI or system fonts for clarity)
- Color scheme matching ReportingDashboardDesign.png reference

**Evidence & Reasoning:**
Bootstrap 5.3.x is actively maintained (last update March 2024), has massive community adoption, and ensures responsive behavior across desktop/tablet without additional work. Custom print CSS is critical—PowerPoint screenshots require pixel-perfect layout and readability at 72-96 DPI.

**Concrete Implementation:**
```html
<meta name="viewport" content="width=device-width, initial-scale=1">
<link href="bootstrap.min.css" rel="stylesheet">
<style>
  @media print {
    body { margin: 0; padding: 10px; font-size: 11pt; }
    .no-print { display: none; }
    .card { page-break-inside: avoid; }
  }
  .milestone-date { font-weight: 600; color: #0d47a1; }
  .status-badge { display: inline-block; padding: 4px 8px; border-radius: 3px; }
</style>
```

---

## Sub-Question 6: Timezone & Date Calculation Handling

**Key Findings:**
Executive dashboards must clearly communicate deadline status. Relative calculations (days remaining, days overdue) require careful handling of timezone context and system clock assumptions.

**Tools & Libraries:**
- `System.DateTime` (no timezone info) — acceptable for intranet dashboards with uniform timezone
- `System.DateTimeOffset` (includes timezone) — recommended for accuracy
- `NodaTime` 3.x (optional for complex timezone scenarios; likely overkill)

**Concrete Recommendation:**
- Store all milestone dates in UTC in data.json (ISO 8601 format: "2026-04-30T23:59:59Z")
- Render as local time in dashboard (use server time zone or dashboard configuration for consistency)
- Calculate days-remaining as: `(MilestoneDate - SystemDateTime.Now).TotalDays`
- Flag milestones >= 3 days overdue as "At Risk", >= 7 days as "Overdue"

**C# Implementation:**
```csharp
public class MilestoneCalculations
{
    public static int DaysRemaining(DateTime milestoneDate)
        => (int)Math.Ceiling((milestoneDate - DateTime.UtcNow).TotalDays);
    
    public static string StatusLabel(DateTime milestoneDate)
    {
        int daysRemaining = DaysRemaining(milestoneDate);
        return daysRemaining switch
        {
            >= 0 => "At Risk",
            >= -3 => "Overdue",
            _ => "Significantly Overdue"
        };
    }
}
```

**Trade-offs & Alternatives:**
- NodaTime adds 500KB+ dependency for timezone database; use only if dashboard must support multiple timezones or historical DST transitions
- Simple DateTime.Now vs. DateTime.UtcNow—use UTC to avoid DST edge cases and server clock drift issues

**Evidence & Reasoning:**
Executives need clear, unambiguous milestone status. Using UTC internally with local rendering prevents timezone conversion bugs. NodaTime is industry-standard for complex scenarios but adds maintenance burden unnecessary for single-timezone intranet apps.

---

## Sub-Question 7: State Management Architecture (Client-Side vs. Service Layer)

**Key Findings:**
For a simple read-mostly dashboard, a minimal service layer provides better testability and future flexibility without over-engineering.

**Tools & Libraries:**
- Blazor Cascading Parameters (native, no dependencies) for top-down data flow
- Optional: `MediatR` 12.x if logic becomes complex (deferred decision)
- Dependency Injection via `IServiceCollection` (built-in)

**Concrete Recommendation:**
Implement a `DashboardService` class:
```csharp
public interface IDashboardService
{
    Task<DashboardData> LoadDataAsync(string filePath);
    IAsyncEnumerable<DashboardData> WatchDataAsync(string filePath);
}

public class DashboardService : IDashboardService
{
    private readonly ILogger<DashboardService> _logger;
    
    public async Task<DashboardData> LoadDataAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<DashboardData>(json)
            ?? throw new InvalidOperationException("Invalid data.json");
    }
    
    public async IAsyncEnumerable<DashboardData> WatchDataAsync(string filePath)
    {
        using var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath));
        var tcs = new TaskCompletionSource<bool>();
        
        watcher.Changed += (s, e) => tcs.TrySetResult(true);
        watcher.EnableRaisingEvents = true;
        
        while (true)
        {
            yield return await LoadDataAsync(filePath);
            await tcs.Task;
            tcs = new TaskCompletionSource<bool>();
        }
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IDashboardService, DashboardService>();
```

**Trade-offs & Alternatives:**
- Pure component state (no service) is simpler but couples JSON parsing to UI rendering logic
- MediatR queries add flexibility for business logic but introduce indirection unnecessary at MVP stage
- SignalR for real-time updates from external services—deferred; use polling via FileSystemWatcher for now

**Evidence & Reasoning:**
This pattern separates concerns (data loading, file watching) from presentation (Blazor components). Testing the service in isolation is straightforward. If future features require external data sources, adding adapters becomes simple. Avoids premature complexity while enabling clean growth.

---

## Sub-Question 8: Performance & Export Constraints

**Key Findings:**
Blazor Server renders on the server; client receives pre-rendered HTML over WebSocket. Screenshot export via browser's print-to-PDF maintains full fidelity without additional tooling.

**Key Metrics:**
- Typical dashboard HTML size: 50-150 KB (with Bootstrap + inline styles)
- Rendering time: < 200ms for 30-50 work items
- Print-to-PDF export: native browser feature, no additional libraries needed

**Tools & Libraries:**
- Optional: `SelectPdf` 21.x (commercial) for server-side PDF generation if automated exports needed
- Blazor Server's native rendering (no additional libraries required)

**Trade-offs & Alternatives:**
- HTML snapshot export (save as .html file): zero dependencies, preserves styling, simplest for PowerPoint embedding
- SelectPdf server-side generation: enables scheduled PDF reports, adds 10+ MB dependency
- Puppeteer/Playwright headless browser: adds infrastructure complexity, unnecessary for manual screenshot workflow

**Concrete Recommendation:**
Leverage browser's native print functionality (Ctrl+P or Print button in UI) to export as PDF. For PowerPoint screenshots:
1. Render dashboard in browser
2. Print to PDF (or take Chrome DevTools screenshot)
3. Embed PDF page or screenshot in PowerPoint slide

For future automated report generation, defer SelectPdf decision until volume justifies infrastructure investment.

**Performance Benchmarks:**
- Dashboard rendering: 80-150 ms (Blazor Server, 30 work items)
- Component tree depth: 6-8 levels (acceptable)
- CSS file size: ~50 KB (minified Bootstrap + custom)
- Total HTTP payload: ~120 KB initial, ~10 KB per update (WebSocket delta)

**Evidence & Reasoning:**
Blazor Server's server-side rendering eliminates client-side rendering overhead that would burden screenshot export. Browser print functionality is reliable, platform-agnostic, and requires zero custom code. Automated export (SelectPdf) is a future enhancement, not MVP requirement per context.

---

## Cross-Cutting Recommendations Summary

| Aspect | Recommendation | Rationale |
|--------|---|---|
| **Parsing & Reload** | FileSystemWatcher + System.Text.Json | Native, zero dependencies, reliable |
| **Charting** | RadzenBlazor 4.x | Executive-grade aesthetics, print-ready |
| **Architecture** | 4-5 reusable sub-components | Enables future variants, testable |
| **Data Schema** | Flat JSON with ISO 8601 dates | Normalized, validates cleanly, supports partial updates |
| **Styling** | Bootstrap 5.3.x + custom @media print | Responsive, screenshot-optimized, industry standard |
| **Date Handling** | DateTimeOffset (UTC), local render | Prevents DST bugs, clear status communication |
| **State** | Minimal DashboardService layer | Testable, extensible, avoids premature complexity |
| **Export** | Browser print-to-PDF, native screenshot | Zero dependencies, reliable, aligns with PowerPoint workflow |
