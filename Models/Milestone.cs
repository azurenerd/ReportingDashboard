namespace AgentSquad.Runner.Models;

public class Milestone
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public MilestoneStatus Status { get; set; }
}

public enum MilestoneStatus
{
    OnTrack,
    AtRisk,
    Completed
}