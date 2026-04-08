namespace AgentSquad.Runner.Services.Models;

public class ProjectData
{
    public string ProjectName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int OverallCompletionPercentage { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<ProjectTask> Tasks { get; set; } = new();
}