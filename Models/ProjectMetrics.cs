using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models
{
    public class ProjectMetrics
    {
        [Range(0, 100)]
        public int CompletionPercentage { get; set; }

        [Required]
        public HealthStatus HealthStatus { get; set; }

        public int VelocityCount { get; set; }

        public int TotalMilestones { get; set; }

        public int CompletedMilestones { get; set; }
    }

    public enum HealthStatus
    {
        OnTrack,
        AtRisk,
        Blocked
    }
}