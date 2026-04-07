namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents the status of a project milestone.
/// </summary>
public enum MilestoneStatus
{
    /// <summary>Milestone has been completed.</summary>
    Completed,

    /// <summary>Milestone is currently in progress.</summary>
    InProgress,

    /// <summary>Milestone is at risk of missing target date.</summary>
    AtRisk,

    /// <summary>Milestone is planned for future execution.</summary>
    Future
}

/// <summary>
/// Represents the status of a work item.
/// </summary>
public enum WorkItemStatus
{
    /// <summary>Work item has been shipped/released.</summary>
    Shipped,

    /// <summary>Work item is currently in progress.</summary>
    InProgress,

    /// <summary>Work item was carried over to next period.</summary>
    CarriedOver
}

/// <summary>
/// Represents the overall health status of a project.
/// </summary>
public enum HealthStatus
{
    /// <summary>Project is on track to meet objectives.</summary>
    OnTrack,

    /// <summary>Project is at risk of missing objectives.</summary>
    AtRisk,

    /// <summary>Project is blocked and unable to proceed.</summary>
    Blocked
}