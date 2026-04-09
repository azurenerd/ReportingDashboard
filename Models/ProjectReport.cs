namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents the complete project reporting snapshot. This is the root aggregate
/// containing all milestone, status, and KPI data for a single reporting period.
/// Immutable data container; no validation logic.
/// </summary>
public class ProjectReport
{
    /// <summary>
    /// Gets the project name. Non-empty, max 100 characters.
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// Gets the reporting period (e.g., "Q2 2026", "2026-04", etc.).
    /// Non-empty, max 50 characters.
    /// </summary>
    public string ReportingPeriod { get; set; }

    /// <summary>
    /// Gets the array of project milestones. Min length 1, sorted by TargetDate recommended.
    /// </summary>
    public Milestone[] Milestones { get; set; }

    /// <summary>
    /// Gets the status snapshot containing shipped, in-progress, and carried-over items.
    /// </summary>
    public StatusSnapshot StatusSnapshot { get; set; }

    /// <summary>
    /// Gets the key-value dictionary of KPI metrics. Optional; values are 0-100 percentages.
    /// Examples: OnTimeDelivery, TeamCapacity, BudgetUtilization.
    /// </summary>
    public Dictionary<string, int> Kpis { get; set; } = new();
}