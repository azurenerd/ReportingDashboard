namespace AgentSquad.Runner.Models;

public class DashboardData
{
    public Project Project { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<WorkItem> WorkItems { get; set; } = new();
}