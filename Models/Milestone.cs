namespace AgentSquad.Runner.Models;

public class Milestone
{
    public string Name { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
}

public enum MilestoneStatus
{
    Completed,
    InProgress,
    AtRisk,
    Future
}