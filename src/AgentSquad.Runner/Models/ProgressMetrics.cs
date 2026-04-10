namespace AgentSquad.Runner.Models;

public class ProgressMetrics
{
    public int TotalPlanned { get; set; }
    public int Completed { get; set; }
    public int InFlight { get; set; }
    public decimal HealthScore { get; set; }
}