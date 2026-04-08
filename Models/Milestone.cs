namespace AgentSquad.Runner.Models;

public class Milestone
{
    public required string Name { get; set; }
    public DateTime TargetDate { get; set; }
    public required MilestoneStatus Status { get; set; }
    public string? Description { get; set; }
}

public enum MilestoneStatus
{
    Completed,
    InProgress,
    AtRisk,
    Future
}