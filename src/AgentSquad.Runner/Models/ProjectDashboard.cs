namespace AgentSquad.Runner.Models;

public class ProjectDashboard
{
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedCompletion { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<WorkItem> Shipped { get; set; } = new();
    public List<WorkItem> InProgress { get; set; } = new();
    public List<WorkItem> CarriedOver { get; set; } = new();
    public ProgressMetrics Metrics { get; set; } = new();
}