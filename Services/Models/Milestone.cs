namespace AgentSquad.Services.Models;

public class Milestone
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public int CompletionPercentage { get; set; } = 0;
}