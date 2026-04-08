namespace AgentSquad.Runner.Models;

public class WorkItem
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required WorkItemStatus Status { get; set; }
    public string? AssignedTo { get; set; }
}