using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents key performance indicators and health metrics for a project.
    /// </summary>
    public class ProjectMetrics
    {
        /// <summary>
        /// Overall project completion percentage (0-100).
        /// </summary>
        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }

        /// <summary>
        /// Current project health status.
        /// </summary>
        [JsonPropertyName("healthStatus")]
        public HealthStatus HealthStatus { get; set; }

        /// <summary>
        /// Number of work items completed this month (velocity indicator).
        /// </summary>
        [JsonPropertyName("velocityThisMonth")]
        public int VelocityThisMonth { get; set; }

        /// <summary>
        /// Total number of planned milestones for the project.
        /// </summary>
        [JsonPropertyName("totalMilestones")]
        public int TotalMilestones { get; set; }

        /// <summary>
        /// Number of milestones completed to date.
        /// </summary>
        [JsonPropertyName("completedMilestones")]
        public int CompletedMilestones { get; set; }
    }
}