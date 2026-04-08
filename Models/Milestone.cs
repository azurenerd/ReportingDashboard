namespace AgentSquad.Models;

public enum MilestoneStatus
{
    Pending,
    InProgress,
    Completed
}

public class Milestone
{
    public string Name { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public int CompletionPercentage { get; set; }
}