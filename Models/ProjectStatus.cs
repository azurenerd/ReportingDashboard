namespace AgentSquad.Runner.Models;

public class ProjectStatus
{
    public List<Milestone> Milestones { get; set; } = new();
    public List<ProjectTask> Tasks { get; set; } = new();
}