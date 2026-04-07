namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents the status of a project milestone.
/// </summary>
public enum MilestoneStatus
{
    /// <summary>
    /// The milestone has been completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The milestone is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The milestone is at risk and may not meet its target date.
    /// </summary>
    AtRisk,

    /// <summary>
    /// The milestone is planned for the future.
    /// </summary>
    Future
}

/// <summary>
/// Represents the status of a work item.
/// </summary>
public enum WorkItemStatus
{
    /// <summary>
    /// The work item has been shipped or completed.
    /// </summary>
    Shipped,

    /// <summary>
    /// The work item is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The work item has been carried over from a previous period.
    /// </summary>
    CarriedOver
}

/// <summary>
/// Represents the overall health status of a project.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// The project is on track to meet its goals.
    /// </summary>
    OnTrack,

    /// <summary>
    /// The project is at risk of delays or issues.
    /// </summary>
    AtRisk,

    /// <summary>
    /// The project is blocked and unable to proceed.
    /// </summary>
    Blocked
}