namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents key performance indicators and health metrics for the project.
/// </summary>
public class ProjectMetrics
{
    /// <summary>
    /// The overall project completion percentage (0-100).
    /// </summary>
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// The current health status of the project (OnTrack, AtRisk, or Blocked).
    /// </summary>
    public HealthStatus HealthStatus { get; set; }

    /// <summary>
    /// The number of work items completed this month.
    /// </summary>
    public int VelocityThisMonth { get; set; }

    /// <summary>
    /// The total number of milestones in the project.
    /// </summary>
    public int TotalMilestones { get; set; }

    /// <summary>
    /// The number of milestones that have been completed.
    /// </summary>
    public int CompletedMilestones { get; set; }
}