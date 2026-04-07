using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents a project milestone with target date and status tracking.
    /// </summary>
    public class Milestone
    {
        /// <summary>
        /// Gets or sets the milestone name or title.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target completion date for this milestone.
        /// </summary>
        [JsonPropertyName("targetDate")]
        public DateTime TargetDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of this milestone.
        /// </summary>
        [JsonPropertyName("status")]
        public MilestoneStatus Status { get; set; }

        /// <summary>
        /// Gets or sets an optional description or details about this milestone.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}