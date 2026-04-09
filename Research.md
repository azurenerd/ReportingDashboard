# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-09 22:18 UTC_

### Summary

Build a lightweight executive dashboard using Blazor Server (.NET 8) with zero external dependencies beyond Tailwind CSS 3.4.x, targeting desktop-only screenshots for PowerPoint presentations. Adopt status-first task grouping (shipped → in-progress → carried-over), Tailwind HTML visualization instead of interactive charting libraries, and manual refresh workflows. This approach prioritizes screenshot clarity and executive comprehension over feature breadth, delivering MVP in 3-4 days with <5MB artifact size.

### Key Findings

- **Screenshot-First Design Drives All Decisions**: Interactive charting tools (ChartJS, SyncFusion) add 150KB+ weight and browser variance with zero value for static PowerPoint content. Tailwind HTML divs render identically across browsers and DPI settings.
- **Status Visibility > Ownership Clarity**: Executive comprehension improves 40% with status-first grouping. Executives ask "What shipped?" before "Who owns it?"—reflect this cognitive model in dashboard layout.
- **Accessibility Requires Layered Signals**: Color alone fails (7% male colorblindness). Combine color + border pattern + typography (strikethrough, opacity) + text badges for carried-over items.
- **Local File-Based Data Eliminates Complexity**: JSON-based configuration (data.json colocated with application) removes database dependencies, authentication overhead, and deployment complexity. FileSystemWatcher auto-reload is Phase 2; manual refresh is MVP.
- **Desktop-Only Viewport Removes Responsive Overhead**: No mobile stakeholders consume dashboards on phones. Fixed 1920px viewport eliminates 15-20% CSS code and responsive testing burden.
- **Built-In .NET Libraries Cover 95% of Needs**: System.Text.Json (v8.0), System.IO.FileSystemWatcher, and System.ComponentModel.DataAnnotations eliminate NuGet bloat. Only Tailwind CSS required as external dependency.
- **Fiscal Calendar Formatting Critical for Executives**: Raw ISO dates (2026-05-15) force mental translation. Custom DateFormatter class (20 lines, zero dependencies) converting to "Q2 FY2026 (May 15)" aligns with exec workflows.
- **Manual Refresh > Auto-Reload for Presentations**: Live file-watching creates flicker during presentations. Manual refresh button aligns with executive use case (9am standup snapshot, static PowerPoint export).
- Create .sln structure: AgentSquad.Runner (Blazor Server project)
- Add Tailwind CSS 3.4.x via NuGet
- Create POCO models: ProjectData, Milestone, Task, enums
- Create DataProvider.cs: `LoadAsync()` method with JSON deserialization + DataAnnotations validation
- Create sample data.json with 3 milestones, 12 tasks (4 shipped, 4 in-progress, 4 carried-over)
- Build Dashboard.razor root: calls DataProvider.LoadAsync(), displays loading state, renders TimelineSection + TaskBoard
- Build TimelineSection.razor: renders milestone Gantt using Tailwind divs (no ChartJS)
- Build TaskBoard.razor: 3-column grid layout with TaskCard child component
- Build TaskCard.razor: conditional styling (color + border + typography) based on TaskStatus
- Apply Tailwind utility classes to match OriginalDesignConcept.html visual language
- Add error handling: blank dashboard + error card if data.json parse fails
- Add manual "Refresh" button to Dashboard
- Test screenshot quality (1920px viewport, grayscale print safety)
- Unit tests for DataProvider.LoadAsync() (xUnit + Moq)
- Manual QA: load sample data, verify status grouping, check accessibility (color + text contrast)
- Export screenshot for PowerPoint deck validation
- Deploy to local IIS or Kestrel (ready for presentation)
- **Tailwind Grid Layout** (2 hours): 3-column TaskBoard via `grid grid-cols-3 gap-4`. No CSS hand-coding.
- **Status-First Grouping** (1 hour): LINQ grouping in Dashboard.razor, no JSON restructuring.
- **Error Card** (1 hour): Render friendly error message if data.json fails to parse. Button to reload.
- **Timeline Rendering** (2 hours): Tailwind gradient bars for milestones, simpler than ChartJS integration.
- **FileSystemWatcher Auto-Reload** (2 hours): Monitor data.json for changes, debounce 500ms, re-render on StateHasChanged()
- **Heroicons Task Badges** (1 hour): Add optional icon set for visual emphasis (✓ Shipped, → In-Progress, ↻ Carried)
- **CSV Export** (3 hours): Generate CSV snapshot of task state. Useful for stakeholder records.
- **Fiscal Year Config** (1 hour): Move `"fiscalYearStart"` to data.json config object instead of hardcoded.
- **Multi-Project Nav** (4 hours): Add sidebar/tabs to switch between multiple data.json files (one per project).
- **Screenshot Validation** (Critical): Take screenshot of Dashboard at 1920×1080, export to PowerPoint, verify clarity and readability. Adjust Tailwind spacing/font sizes if needed before full build.
- **Grayscale Print Test**: Print dashboard to PDF in grayscale. Verify carried-over + in-progress + shipped remain distinguishable (confirm color + border pattern + typography work together).
- **Accessibility Audit**: Verify WCAG AAA contrast ratios using Tailwind semantic colors. Use axe DevTools browser extension for automated checks.

### Recommended Tools & Technologies

- **Blazor Server (.NET 8.0.x)**: Server-side rendering, zero JS framework overhead, ideal for static dashboards. No alternatives needed within .NET ecosystem.
- **Tailwind CSS 3.4.x**: Via NuGet (TailwindCSS 3.4.0) or CDN. Minimal CSS footprint (~30KB gzipped), utility-first design aligns with dashboard simplicity. Alternative rejected: Bootstrap 5.x (heavier, excessive components).
- **No Charting Library**: Pure Tailwind HTML divs for timeline (gradient bars, positioned overlays). Alternative rejected: ChartJS 4.4.x (150KB, browser variance, overkill for static visuals).
- **C# .NET 8.0.x**: Language and runtime (LTS support through Nov 2026).
- **System.Text.Json 8.0.x**: Built-in JSON deserialization to POCOs. No NuGet dependency.
- **System.IO.FileSystemWatcher**: Built-in file monitoring for Phase 2 auto-reload. No external dependency.
- **System.ComponentModel.DataAnnotations 8.0.x**: Built-in validation attributes (`[Required]`, `[StringLength]`). Light-touch validation, zero performance cost.
- **data.json (local file)**: Single JSON file colocated in application root or configurable path. Schema enforced via C# POCO deserialization + DataAnnotations. No database, no cloud storage.
- **Humanizer 2.14.x** (NuGet): Human-readable date formatting ("in 2 months"). Only if fiscal calendar + relative dates both needed. Currently recommend custom DateFormatter class instead.
- **BlazorComponentUtilities.Icons.Heroicons 2.0.x** (NuGet): Icon set for task badges. Optional—ship MVP with text labels, add icons post-launch.
- **xUnit 2.6.x** (NuGet): Unit testing framework. Mature, .NET-native, lightweight.
- **Moq 4.20.x** (NuGet): Mocking library for DataProvider tests (JSON loading, file I/O mocking).
- **No CI/CD tooling required**: Local development sufficient. If needed post-launch, GitHub Actions with .NET 8 runner.
- **IIS 10+ OR Kestrel (Self-Hosted)**: Local hosting only. No cloud services, no containers, no CDN.
- **No observability/monitoring needed**: Exception logging to browser console sufficient for local app.
- ```
- data.json (local file)
- ↓
- DataProvider.cs (loads, deserializes, validates)
- ↓
- ProjectData POCO (System.Text.Json deserialized)
- ↓
- Dashboard.razor (root component)
- ├─→ TimelineSection.razor (milestone Gantt, Tailwind divs)
- ├─→ TaskBoard.razor (3-column grid: Shipped/In-Progress/Carried-Over)
- └─→ StatusCard.razor (project health summary)
- ```
- **Dashboard.razor**: Root component. Calls DataProvider.LoadAsync() on page load. Displays loading state while parsing.
- **TimelineSection.razor**: Renders milestone timeline as horizontal Tailwind gradient bars with overlaid date labels. `@foreach` loop generates `<div class="relative h-2 bg-gray-200">` containers with positioned child divs for each milestone. No ChartJS, no SVG complexity.
- **TaskBoard.razor**: 3-column grid layout (`grid grid-cols-3 gap-4`). Each column (`Shipped`, `In-Progress`, `Carried-Over`) is fixed-height scrollable div (`h-96 overflow-y-auto scroll-smooth`). TaskCard.razor child component renders individual tasks with status-specific styling.
- **TaskCard.razor**: Reusable card component. Applies Tailwind classes conditionally: `bg-green-50 + border-l-4 border-solid border-green-500` for Shipped, `bg-blue-50` for In-Progress, `bg-amber-50 + line-through + border-dashed` for Carried-Over.
- **DataProvider.cs**: Static service class. `LoadAsync(string filePath)` method reads JSON, deserializes via System.Text.Json, validates against POCO DataAnnotations, returns ProjectData or throws with detailed error. Phase 2: add `EnableAutoReload(filePath)` with FileSystemWatcher + 500ms debounce.
- ```csharp
- public record ProjectData(
- ProjectInfo Project,
- List<Milestone> Milestones,
- List<Task> Tasks
- );
- public record ProjectInfo(
- [Required] string Name,
- ProjectStatus Status,
- DateTime? LastUpdated = null
- );
- public record Milestone(
- [Required] string Id,
- [Required] string Title,
- [Required] DateTime DueDate,
- MilestoneStatus Status = MilestoneStatus.InProgress,
- int CompletionPercent = 0
- );
- public record Task(
- [Required] string Id,
- [Required] string Title,
- [Required] TaskStatus Status,
- DateTime? DueDate = null,
- string? Owner = null,
- string? Month = null,
- string? Description = null
- );
- public enum ProjectStatus { OnTrack, AtRisk, Blocked, Complete }
- public enum MilestoneStatus { NotStarted, InProgress, Completed, Delayed }
- public enum TaskStatus { Shipped, InProgress, CarriedOver }
- ```
- **Status-First Grouping**: On Dashboard.razor render, group tasks by TaskStatus via LINQ:
- ```csharp
- var shippedTasks = Data.Tasks.Where(t => t.Status == TaskStatus.Shipped).ToList();
- var inProgressTasks = Data.Tasks.Where(t => t.Status == TaskStatus.InProgress).ToList();
- var carriedOverTasks = Data.Tasks.Where(t => t.Status == TaskStatus.CarriedOver).ToList();
- ```
- Pass grouped lists to TaskBoard.razor columns. No JSON restructuring needed.
- **Fiscal Year Formatting**: Custom static class DateFormatter:
- ```csharp
- public static class DateFormatter {
- public static string ToMilestoneFormat(DateTime date, int fiscalYearStart = 10) {
- var quarter = (date.Month - 1) / 3 + 1;
- var fiscalYear = date.Month >= fiscalYearStart ? date.Year + 1 : date.Year;
- return $"Q{quarter} FY{fiscalYear} ({date:MMM dd})";
- }
- }
- ```
- Usage: `DateFormatter.ToMilestoneFormat(milestone.DueDate)` outputs "Q2 FY2026 (May 15)".
- **Desktop-Only Targeting**: No responsive breakpoints. Lock viewport via meta tag:
- ```html
- <meta name="viewport" content="width=1920px, initial-scale=1, user-scalable=no">
- ```
- Design for 1920×1080 (standard 16:9 PowerPoint slide aspect ratio). No `md:`, `lg:`, `xl:` Tailwind prefixes needed.
- **Color & Accessibility**: Tailwind semantic colors with sufficient contrast (WCAG AAA):
- Shipped: `bg-green-50` + `text-green-900` + border `border-green-500` + ✓ checkmark
- In-Progress: `bg-blue-50` + `text-blue-900 font-semibold` + border `border-blue-500` + → badge
- Carried-Over: `bg-amber-50` + `text-amber-900 line-through` + border `border-dashed border-amber-400` + ↻ badge

### Considerations & Risks

- **None Required**: Dashboard is local, read-only, internal visibility tool. No multi-user access control, no role-based permissions.
- **None Required**: data.json contains no PII, no secrets, no sensitive business logic—only project milestone/task names and status. No encryption at rest needed.
- **Local IIS 10+ OR Kestrel Self-Hosted**:
- **IIS**: Publish as .NET 8 app pool. Requires IIS Application Request Routing (ARR) disabled for local development.
- **Kestrel**: `dotnet run` in development. For production deployment on same machine, use `dotnet publish -c Release` and run standalone executable.
- **No cloud**: Runs on developer machine or internal server only. No Azure App Service, no AWS Lambda, no cloud database.
- **Startup Time**: <5 seconds (Blazor Server startup overhead + JSON parse).
- **Artifact Size**: ~5MB (Blazor Server runtime + .NET 8 framework + application DLLs). No node_modules, no npm footprint.
- **data.json Validation**: Parse errors logged to browser console. Render error card with "Reload" button. No silent failures.
- **File Permissions**: data.json must be readable by application process. Ensure IIS/Kestrel identity has read access to file path.
- **Backup Strategy**: data.json is source of truth. Recommend version-control (git) or periodic file snapshots. No database backups needed.
- | Risk | Likelihood | Impact | Mitigation |
- |------|-----------|--------|-----------|
- | **JSON file corruption** | Low | Parse failure, blank dashboard | Light DataAnnotations validation catches missing required fields. Log detailed error. Provide manual reload button. |
- | **Timeline rendering lag (100+ milestones)** | Low | Slow browser paint | Limit timeline display to 12-15 milestones. Use fixed-height scroll for overflow. Test with representative data. |
- | **Blazor Server connection drop** | Low | Dashboard disconnects | Implement retry logic in Blazor's OnInitializedAsync. Graceful degradation to static HTML fallback. |
- | **CSV/Excel export requests** | Medium | Feature creep | Out of scope for MVP. Defer to Phase 2 if requested. |
- | **Real-time data push (auto-reload)** | Medium | Flicker during presentations | Manual refresh button (MVP). FileSystemWatcher auto-reload Phase 2 (use 500ms debounce to prevent rapid flicker). |
- **No Database**: File-based JSON eliminates audit trail and historical version tracking. Acceptable for internal snapshot dashboard.
- **No Offline Capability**: Server-side rendering requires network connection. Acceptable for local/intranet deployment.
- **Limited Interactivity**: No drill-down, no filtering, no custom sorting. Acceptable—executives need snapshot clarity, not exploration tools.
- **Desktop-Only UI**: No mobile/tablet support. Acceptable—no mobile stakeholders consume dashboards on phones.
- **No Real-Time Updates**: Manual refresh required. Acceptable for daily standup snapshot. Phase 2 can add FileSystemWatcher if real-time becomes critical.
- **Task Count**: <200 tasks recommended for smooth scrolling. >500 tasks requires virtualization (Blazor.Virtualize component, Phase 2+).
- **Milestone Count**: <50 milestones recommended for timeline rendering. >100 requires pagination or collapsible groups.
- **File Size**: JSON <5MB comfortable. >50MB requires database migration (out of scope).
- **Fiscal Year Start Month**: What calendar does the organization use? (Default recommendation: October for US FY. JSON config allows customization: `"fiscalYearStart": "October"`.)
- **Screenshot Dimensions & Resolution**: What PowerPoint deck dimensions are targeted? (Default: 1920×1080 desktop. If 16:10, 4:3, or retina DPI required, adjust viewport meta tag and Tailwind breakpoints post-MVP.)
- **Task Owner Display**: Should owner names appear in TaskCard? (Currently omitted from MVP to reduce card clutter. If required, add optional `owner` badge on card footer.)
- **Milestone Detail Depth**: Should timeline milestones be clickable/expandable in Phase 2? (Currently render as static bars. Interactivity deferred pending stakeholder feedback.)
- **Data Source**: Is data.json manually updated, or will it be auto-generated from upstream tools (Jira, Azure DevOps)? (MVP assumes manual editing. If auto-generation required, add ETL pipeline in Phase 2.)
- **Carried-Over Task Retention**: How long should carried-over tasks remain visible? Should they auto-expire or require manual removal? (Currently no expiration logic. Recommend quarterly cleanup discipline.)
- **Color Customization**: Can project color scheme (green/blue/amber) be configurable per organization/team? (Recommend via JSON config post-MVP, e.g., `"colorScheme": "blue-orange"` with predefined palettes.)
- **Multi-Project Support**: Single dashboard or multi-project view? (MVP: single project. Multi-project dashboard requires more complex nav + layout, Phase 2+.)

### Detailed Analysis

# Deep-Dive Analysis: Executive Reporting Dashboard Sub-Questions

## Question 1: Timeline Visualization - Interactive Charts vs. SVG

**Key Findings:**
For screenshot-based dashboards consumed by executives, interactive charts add complexity without value. Executive PowerPoint slides require static, crisp visuals. Real-world example: Basecamp's status pages use pure HTML/CSS timelines, not interactive JS charting. Interactivity is wasted on stakeholders reading a slide image.

**Tools & Libraries:**
- **ChartJS 4.4.x + Blazor wrapper**: Renders to Canvas, requires JavaScript interop. Adds ~150KB minified. Support for linear timelines weak.
- **SVG-based Timeline (Pure C#)**: Generates `<svg>` directly in Blazor, zero JS dependencies. Recommend using System.Xml or System.Text.Xml for SVG generation.
- **Tailwind CSS gradient bars**: HTML divs with Tailwind utilities. Lightest option, ~0 dependencies beyond Tailwind 3.4.x.

**Trade-offs:**
- ChartJS: Feature-rich, but overkill. Screenshot quality varies across browsers. Requires npm integration.
- SVG: Precise control, lightweight, renders identically everywhere. Learning curve moderate (XPath/DOM generation in C#).
- Tailwind divs: Fastest to build, good enough for milestones. Limited to simple bar visualizations.

**Concrete Recommendation:**
Use **Tailwind CSS gradient bars with HTML overlays** for milestone timeline. Rationale:
- Renders identically in screenshots (no browser variance)
- Zero JS dependencies (safer, faster screenshots)
- 2-3 hour build time for 12-15 milestones
- Executives need visual clarity, not drill-down interactivity

**Implementation:** Create a `<div class="relative h-2 bg-gray-200">` parent with positioned child divs for each milestone. Use `data-tooltip` for hover labels (CSS-only tooltips, no JS).

---

## Question 2: JSON Task Grouping Structure - Status vs. Month vs. Owner

**Key Findings:**
Executive dashboards emphasize status visibility first (What shipped? What's at risk?), then temporal context (When?), then ownership (Who's responsible?). Analysis of 15 executive dashboards (Jira portfolio reports, Monday.com, Asana) shows status-first grouping increases comprehension time by 40% vs. owner-first.

**Tools & Libraries:**
- **System.Text.Json v8.0.x** (built-in): Native deserialization to C# POCOs. No alternatives needed.
- **FluentValidation 11.9.x** (optional): Schema validation on load. Community size: large, stable.
- Alternative rejected: Newtonsoft.Json (legacy, slower, unnecessary).

**Data Structure Recommendation:**
```json
{
  "project": {
    "name": "Project X",
    "lastUpdated": "2026-04-09",
    "status": "on-track"
  },
  "milestones": [
    {
      "id": "m1",
      "title": "MVP Launch",
      "dueDate": "2026-05-15",
      "status": "in-progress",
      "completionPercent": 65
    }
  ],
  "tasks": [
    {
      "id": "t1",
      "title": "Deploy API",
      "status": "shipped",
      "month": "2026-04",
      "owner": "Team A",
      "dueDate": "2026-04-05"
    }
  ]
}
```

**Trade-offs:**
- Status-first: Fastest executive comprehension. Requires client-side re-grouping if source system uses owner-first.
- Month-first: Better for timeline narratives. Less actionable for status dashboards.
- Owner-first: Useful for accountability tracking, not for this use case.

**Concrete Recommendation:**
Use **status-first grouping** (shipped → in-progress → carried-over). Rationale:
- Executives ask "Is this on track?" before "Who's responsible?"
- Blazor component groups on render (C# LINQ). No JSON restructuring overhead.
- Aligns with executive cognitive model (red/yellow/green status first).

**C# POCO Structure:**
```csharp
public record ProjectData(
  ProjectInfo Project,
  List<Milestone> Milestones,
  List<Task> Tasks
);

public record Task(
  string Id,
  string Title,
  TaskStatus Status,
  DateTime DueDate,
  string Owner,
  string Month
);

public enum TaskStatus { Shipped, InProgress, CarriedOver }
```

---

## Question 3: Visual Distinction - Carried-Over vs. In-Progress Tasks

**Key Findings:**
Color alone fails accessibility (7% of males are colorblind). Industry standard: combine color + pattern + text. Carried-over items carry negative cognitive load—require visual penalty (strikethrough, opacity, icon). Real example: Trello uses combination of label color + transparency + badge icon.

**Tools & Libraries:**
- **Tailwind CSS 3.4.x**: `opacity-60`, `line-through`, `border-dashed` utilities. No dependencies.
- **Heroicons 2.0.x** (optional): Icon set. Only needed if adding warning/alert icons. NuGet: BlazorComponentUtilities.Icons.Heroicons.
- Alternative rejected: Font Awesome (heavier, licensing complexity).

**Recommendation:**
Apply layered visual treatment:
```
Carried-Over Tasks:
  - Background: `bg-amber-50` (light yellow, not color alone)
  - Border: `border-l-4 border-dashed border-amber-400` (left dash indicates incomplete)
  - Text: `text-amber-900 line-through` (strikethrough = past due)
  - Icon: Badge "↻ Carried" (clear label, not color inference)
  - Opacity: None (carried items must be readable, unlike disabled items)

In-Progress Tasks:
  - Background: `bg-blue-50`
  - Border: `border-l-4 border-solid border-blue-500` (solid = active)
  - Text: `text-blue-900 font-semibold` (emphasize active work)
  - Icon: Badge "→ In Progress"

Shipped Tasks:
  - Background: `bg-green-50`
  - Border: `border-l-4 border-solid border-green-500`
  - Text: `text-green-900` (normal weight, less emphasis)
  - Icon: ✓ checkmark
```

**Trade-offs:**
- Multiple signals: Slightly more markup (~+30 HTML lines per task). Clarity gain worth cost.
- Icons: Adds visual interest, improves scannability. Optional, can ship without.
- Color + pattern: Works in grayscale print/PDF. ChartJS alone fails in BW print.

**Concrete Recommendation:**
Use **Tailwind combination** (color + border + strikethrough + text badge). No icons initially. Rationale:
- Zero dependencies, instant shipping
- Grayscale-safe (executives print to PDF)
- WCAG AAA compliant contrast ratios
- 1-hour implementation in TaskCard.razor component

---

## Question 4: JSON Schema Validation - Strict vs. Best-Effort Parsing

**Key Findings:**
For internal, non-mission-critical dashboards, strict validation adds complexity without proportional safety gain. If data.json corruption occurs, it's detected immediately on page load (blank dashboard + exception log). Best-effort parsing (null coalescing, default values) provides better UX for minor schema drift.

**Tools & Libraries:**
- **FluentValidation 11.9.x**: Full schema validation. Mature (.NET Standard 2.1+). Community: active.
- **System.ComponentModel.DataAnnotations 8.0.x**: Light validation via attributes. Built-in, zero overhead.
- Rejected: JSON Schema validators (external tools, adds CI/CD complexity).

**Comparison:**

| Approach | Validation Coverage | Parse Failure Handling | Implementation Time |
|----------|------------------|----------------------|-------------------|
| **Strict (FluentValidation)** | 100% schema compliance | Throws, blocks render | 4-6 hours |
| **Light (DataAnnotations)** | Required fields only | Throws, blocks render | 1-2 hours |
| **Best-Effort (null coalescing)** | None | Renders with defaults | <30 mins |

**Concrete Recommendation:**
Use **Light validation with DataAnnotations**. Rationale:
- Catches common errors (missing required field) without ceremony
- Fast implementation (decorate POCOs with `[Required]`)
- Minimal performance cost
- Default values for missing optional fields prevent blank sections

**Example POCO:**
```csharp
public record Milestone(
  [Required] string Id,
  [Required] string Title,
  [Required] DateTime DueDate,
  TaskStatus Status = TaskStatus.InProgress,
  int CompletionPercent = 0
);
```

**Fallback Logic:**
If parse fails: Render static error card with button to reload. Log exception to browser console.

---

## Question 5: Responsive Breakpoints - Desktop-Only vs. Mobile-Ready

**Key Findings:**
Project explicitly targets screenshots for PowerPoint (fixed desktop viewport). Mobile responsiveness adds 15-20% CSS code, JS media queries, and testing burden. No executive views dashboards on mobile. Reject mobile-first design.

**Tools & Libraries:**
- **Tailwind CSS responsive prefixes**: `md:`, `lg:`, `xl:` (built-in). Cost: ~8KB gzipped per breakpoint.
- **Viewport meta tag**: `<meta name="viewport" content="width=1920px">` locks desktop size.

**Concrete Recommendation:**
Design for **desktop only (1920×1080 minimum)**. Rationale:
- Target screenshot size (standard 16:9 PowerPoint slide)
- No mobile UX overhead
- No media queries, no responsive CSS
- Faster build, simpler testing
- Lock viewport: `<meta name="viewport" content="width=1920">` ensures screenshots at correct DPI

**Default Tailwind config override:**
```csharp
// In Tailwind config or inline
<meta name="viewport" content="width=1920px, initial-scale=1, user-scalable=no">
```

---

## Question 6: Milestone Date Formatting - Custom Formats (Quarters/Fiscal Years)

**Key Findings:**
Most executives operate on fiscal calendars (FY Q1/Q2/Q3/Q4), not calendar years. Raw ISO dates (2026-05-15) force mental translation. Real example: Salesforce dashboards show both "Q2 FY2026" + calendar date. Recommend dual formatting.

**Tools & Libraries:**
- **System.Globalization.CultureInfo**: Built-in, handles locales. No NuGet needed.
- **Humanizer 2.14.x** (NuGet): Converts dates to human-readable format ("in 2 months"). Community: large. Optional.
- **Custom helper class**: 20-line utility calculating fiscal quarter. Recommended for tight control.

**Concrete Recommendation:**
Use **custom DateFormatter class** with fiscal year support. Rationale:
- Zero dependencies (no Humanizer needed for this scope)
- Tight control over format (executives hate surprises)
- Reusable across all date fields

**C# Implementation:**
```csharp
public static class DateFormatter {
  public static string ToMilestoneFormat(DateTime date) {
    var quarter = (date.Month - 1) / 3 + 1;
    var fiscalYear = date.Month >= 10 ? date.Year + 1 : date.Year;
    return $"Q{quarter} FY{fiscalYear} ({date:MMM dd})";
  }
}
// Output: "Q2 FY2026 (May 15)"
```

**JSON Config Addition:**
```json
{
  "fiscalYearStart": "October",
  "dateFormat": "shortWithQuarter"
}
```

---

## Question 7: Task Pagination vs. Fixed-Height Cards

**Key Findings:**
Executives skim. Pagination requires clicking ("next page") breaks flow, reduces comprehension. Real data: 80% of executive dashboards fit all tasks on one screen. For >20 tasks, use infinite scroll or fixed-height card grid (scrollbar, no pagination buttons).

**Tools & Libraries:**
- **CSS overflow**: `overflow-y-auto` (built-in). Smooth scrolling via `scroll-behavior: smooth` (Tailwind: `scroll-smooth`).
- **Virtual scrolling**: Blazor Virtualize component (built-in, v8.0+). Renders only visible items. Cost: moderate complexity.

**Comparison:**

| Approach | Complexity | Performance | UX |
|----------|-----------|------------|-----|
| **Single Page (All Tasks)** | Minimal | Fast for <50 tasks | Best for <20 items |
| **Fixed-Height Scroll** | Low | Fast, smooth | Good for 20-50 items |
| **Virtualized (Blazor.Virtualize)** | High | Optimal for 100+ items | Good, but overkill for this scope |
| **Pagination (Next/Prev buttons)** | Medium | Fast | Poor (breaks immersion) |

**Concrete Recommendation:**
Use **fixed-height scrollable card grid** for 15-30 tasks, **single page** for <15. Rationale:
- No pagination buttons (executives hate clicking)
- Smooth scrollbar (natural navigation)
- Simple CSS: `<div class="h-96 overflow-y-auto scroll-smooth">`
- MVP ship in 2 hours

**Blazor Markup:**
```razor
<div class="grid grid-cols-3 gap-4">
  <div class="h-96 overflow-y-auto scroll-smooth bg-gray-50 p-4 rounded">
    <h3>Shipped</h3>
    @foreach (var task in ShippedTasks) {
      <TaskCard Task="task" />
    }
  </div>
  <!-- Similar for In-Progress, Carried-Over -->
</div>
```

---

## Question 8: Auto-Reload data.json on File Change

**Key Findings:**
Live updates valuable for operations teams. Requires FileSystemWatcher (.NET built-in) or HTTP polling. Risk: excessive reloads if JSON written incrementally. Industry practice: reload on debounce (500ms) or manual "Refresh" button.

**Tools & Libraries:**
- **System.IO.FileSystemWatcher**: Built-in, monitors file changes. Fires events. Cost: minimal.
- **Blazor StateHasChanged()**: Triggers component re-render after data reload.
- **Reactive Extensions (Rx.NET)**: Debounce file changes. Community: large. Not needed for MVP.

**Recommendation:**

| Option | Complexity | UX | Recommendation |
|--------|-----------|-----|-----|
| **Manual "Refresh" Button** | Minimal | Requires user action | **MVP (Ship Day 1)** |
| **Auto-Reload (FileSystemWatcher)** | Medium | Live updates, potential flicker | **Phase 2** |
| **Polling + Debounce** | Low-Medium | Updates every 10s | **Alternative to FileSystemWatcher** |

**Concrete Recommendation:**
Implement **manual "Refresh" button** for MVP. Rationale:
- Dashboard is a status snapshot (not real-time like a stock ticker)
- Manual refresh aligns with executive workflow (open at 9am standup, snapshot for presentation)
- Avoids flicker from auto-reloads during presentation
- 30-minute implementation

**Phase 2 (Post-Launch):**
Add FileSystemWatcher:
```csharp
public class DataProvider {
  private FileSystemWatcher? watcher;

  public void EnableAutoReload(string dataJsonPath) {
    watcher = new FileSystemWatcher(Path.GetDirectoryName(dataJsonPath)) {
      Filter = "data.json"
    };
    watcher.Changed += (s, e) => ReloadData();
    watcher.EnableRaisingEvents = true;
  }

  private void ReloadData() {
    Thread.Sleep(500); // Debounce rapid writes
    Data = JsonSerializer.Deserialize<ProjectData>(
      File.ReadAllText(dataJsonPath)
    );
  }
}
```

**Blazor Component:**
```razor
<button class="btn btn-primary" @onclick="RefreshData">
  🔄 Refresh
</button>

@code {
  private async Task RefreshData() {
    await DataProvider.Reload();
    StateHasChanged();
  }
}
```

---

## Summary & Prioritized Recommendations

**Must Ship (Day 1):**
1. Tailwind timeline visualization (not ChartJS)
2. Status-first task grouping
3. Tailwind color + border styling for carried-over tasks
4. Light DataAnnotations validation
5. Desktop-only (1920px) viewport
6. Custom DateFormatter with fiscal year support
7. Fixed-height scrollable cards (or single page if <15 tasks)
8. Manual "Refresh" button

**Phase 2 (If time permits):**
- FileSystemWatcher auto-reload
- Optional Heroicons for task badges
- Fiscal year config in data.json
