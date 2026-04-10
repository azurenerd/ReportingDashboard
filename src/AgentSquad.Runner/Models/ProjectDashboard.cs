namespace AgentSquad.Runner.Models;

/// <summary>
/// Root aggregate for the executive dashboard. Contains project metadata,
/// milestones, work items organized by status, and calculated metrics.
/// </summary>
public class ProjectDashboard
{
    /// <summary>
    /// Project name (required). Example: "Project Alpha"
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Optional project description. Example: "Q2 2026 Initiative"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Project start date in ISO 8601 format.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Target project completion date in ISO 8601 format.
    /// </summary>
    public DateTime PlannedCompletion { get; set; }

    /// <summary>
    /// Major project milestones (typically 5-10 items).
    /// Initialized to empty list for safe null-coalescing.
    /// </summary>
    public List<Milestone> Milestones { get; set; } = new();

    /// <summary>
    /// Work items that have been shipped/completed.
    /// </summary>
    public List<WorkItem> Shipped { get; set; } = new();

    /// <summary>
    /// Work items currently in active development.
    /// </summary>
    public List<WorkItem> InProgress { get; set; } = new();

    /// <summary>
    /// Work items that have been deferred or carried over from previous periods.
    /// </summary>
    public List<WorkItem> CarriedOver { get; set; } = new();

    /// <summary>
    /// Calculated aggregate metrics (total, completed, in-flight, health score).
    /// Populated by ProjectDataService during JSON deserialization.
    /// </summary>
    public ProgressMetrics Metrics { get; set; } = new();
}