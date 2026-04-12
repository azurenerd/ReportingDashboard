namespace AgentSquad.Runner.Models;

public class Milestone
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public List<Checkpoint>? Checkpoints { get; set; }
    public MilestoneMarker? PocMilestone { get; set; }
    public MilestoneMarker? ProductionRelease { get; set; }
}