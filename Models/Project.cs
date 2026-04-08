namespace AgentSquad.Runner.Models;

public class Project
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime TargetEndDate { get; set; }
    public int CompletionPercentage { get; set; }
    public HealthStatus HealthStatus { get; set; }
    public int VelocityThisMonth { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<WorkItem> WorkItems { get; set; } = new();
}

public enum HealthStatus
{
    OnTrack,
    AtRisk,
    Blocked
}