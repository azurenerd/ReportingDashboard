namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents the status of a project milestone.
    /// </summary>
    public enum MilestoneStatus
    {
        Completed = 0,
        InProgress = 1,
        AtRisk = 2,
        Future = 3
    }

    /// <summary>
    /// Represents the status of a work item.
    /// </summary>
    public enum WorkItemStatus
    {
        Shipped = 0,
        InProgress = 1,
        CarriedOver = 2
    }

    /// <summary>
    /// Represents the overall health status of a project.
    /// </summary>
    public enum HealthStatus
    {
        OnTrack = 0,
        AtRisk = 1,
        Blocked = 2
    }
}