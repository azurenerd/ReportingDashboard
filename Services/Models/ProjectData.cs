namespace AgentSquad.Services.Models;

public class ProjectData
{
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
}