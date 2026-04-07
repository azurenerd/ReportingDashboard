using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents the complete project data including milestones, work items, and health metrics.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the project name or title.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional description of the project.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the project start date.
        /// </summary>
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the target project end date.
        /// </summary>
        [JsonPropertyName("targetEndDate")]
        public DateTime TargetEndDate { get; set; }

        /// <summary>
        /// Gets or sets the overall project completion percentage (0-100).
        /// </summary>
        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the current health status of the project.
        /// </summary>
        [JsonPropertyName("healthStatus")]
        public HealthStatus HealthStatus { get; set; }

        /// <summary>
        /// Gets or sets the number of work items completed this month (velocity metric).
        /// </summary>
        [JsonPropertyName("velocityThisMonth")]
        public int VelocityThisMonth { get; set; }

        /// <summary>
        /// Gets or sets the collection of project milestones.
        /// </summary>
        [JsonPropertyName("milestones")]
        public List<Milestone> Milestones { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of tracked work items.
        /// </summary>
        [JsonPropertyName("workItems")]
        public List<WorkItem> WorkItems { get; set; } = new();
    }
}