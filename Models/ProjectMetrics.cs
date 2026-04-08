namespace AgentSquad.Runner.Models
{
    public class ProjectMetrics
    {
        public int CompletionPercentage { get; set; }
        public HealthStatus HealthStatus { get; set; }
        public int VelocityThisMonth { get; set; }
        public int MilestoneCount { get; set; }
        public int TargetMilestoneCount { get; set; }
    }

    public enum HealthStatus
    {
        OnTrack,
        AtRisk,
        Blocked
    }
}