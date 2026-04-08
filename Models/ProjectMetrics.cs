using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models
{
    public class ProjectMetrics
    {
        public int CompletionPercentage { get; set; }
        public HealthStatus HealthStatus { get; set; }
        public int VelocityCount { get; set; }
    }
}