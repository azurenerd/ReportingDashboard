namespace AgentSquad.Runner.Models;

public class Milestone
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TargetDate { get; set; }
    public string Status { get; set; } = string.Empty;
}