namespace AgentSquad.Runner.Models;

public class WorkItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkItemStatus Status { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}

public enum WorkItemStatus
{
    Shipped,
    InProgress,
    CarriedOver
}