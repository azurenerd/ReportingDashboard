namespace AgentSquad.Runner.Models;

public class WorkItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Owner { get; set; }
}