namespace AgentSquad.Runner.Models;

public class ProjectTask
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}

public enum TaskStatus
{
    Completed,
    InProgress,
    CarriedOver
}