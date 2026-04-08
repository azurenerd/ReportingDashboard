namespace AgentSquad.Runner.Services.Models;

public class ProjectTask
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int EstimatedDays { get; set; }
    public string RelatedMilestone { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
}