namespace AgentSquad.Runner.Services.Models;

public class ProjectTask
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
}