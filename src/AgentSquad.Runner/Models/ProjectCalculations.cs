namespace AgentSquad.Runner.Models;

public class ProjectCalculations
{
    public int PercentageComplete { get; set; }
    public int TotalStoryPoints { get; set; }
    public int ShippedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CarriedOverCount { get; set; }
    public double VelocityPerSprint { get; set; }
}