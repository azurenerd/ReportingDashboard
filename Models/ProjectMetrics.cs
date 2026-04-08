using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    public class ProjectMetrics
    {
        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }

        [JsonPropertyName("healthStatus")]
        public HealthStatus HealthStatus { get; set; }

        [JsonPropertyName("velocityThisMonth")]
        public int VelocityThisMonth { get; set; }

        [JsonPropertyName("completedMilestones")]
        public int CompletedMilestones { get; set; }

        [JsonPropertyName("milestoneCount")]
        public int MilestoneCount { get; set; }

        [JsonPropertyName("targetMilestoneCount")]
        public int TargetMilestoneCount { get; set; }
    }

    public enum HealthStatus
    {
        OnTrack,
        AtRisk,
        Blocked
    }
}