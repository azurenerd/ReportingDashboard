using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models
{
    public class ProjectMetrics
    {
        public int CompletionPercentage { get; set; }
        public HealthStatus HealthStatus { get; set; }
        public int VelocityThisMonth { get; set; }
        public int TotalMilestones { get; set; }
        public int CompletedMilestones { get; set; }
    }
}