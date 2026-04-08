namespace AgentSquad.Runner.Models;

public class Project
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetEndDate { get; set; }
    public int CompletionPercentage { get; set; }
    public required HealthStatus HealthStatus { get; set; }
    public int VelocityThisMonth { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<WorkItem> WorkItems { get; set; } = new();
}