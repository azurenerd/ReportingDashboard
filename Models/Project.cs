using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents the root project data structure containing milestones, work items, and metrics.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the project name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the project description.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the project start date.
        /// </summary>
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the target end date for the project.
        /// </summary>
        [JsonPropertyName("targetEndDate")]
        public DateTime TargetEndDate { get; set; }

        /// <summary>
        /// Gets or sets the overall project completion percentage (0-100).
        /// </summary>
        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the overall health status of the project.
        /// </summary>
        [JsonPropertyName("healthStatus")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HealthStatus HealthStatus { get; set; }

        /// <summary>
        /// Gets or sets the velocity (work items completed) in the current month.
        /// </summary>
        [JsonPropertyName("velocityThisMonth")]
        public int VelocityThisMonth { get; set; }

        /// <summary>
        /// Gets or sets the collection of project milestones.
        /// </summary>
        [JsonPropertyName("milestones")]
        public List<Milestone> Milestones { get; set; } = new List<Milestone>();

        /// <summary>
        /// Gets or sets the collection of work items for the project.
        /// </summary>
        [JsonPropertyName("workItems")]
        public List<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    }
}