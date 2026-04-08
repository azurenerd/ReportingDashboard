namespace AgentSquad.Runner.Models;

public enum MilestoneStatus
{
    Completed,
    InProgress,
    AtRisk,
    Future
}

public enum WorkItemStatus
{
    Shipped,
    InProgress,
    CarriedOver
}

public enum HealthStatus
{
    OnTrack,
    AtRisk,
    Blocked
}