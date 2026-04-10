namespace AgentSquad.Runner.Models;

public class WorkItem
{
    public string Title { get; set; }
    public WorkItemStatus Status { get; set; }
    public string Assignee { get; set; }
}