using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents a project milestone with target date and status.
    /// </summary>
    public class Milestone
    {
        /// <summary>
        /// Gets or sets the name of the milestone.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the target date for milestone completion.
        /// </summary>
        [JsonPropertyName("targetDate")]
        public DateTime TargetDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the milestone.
        /// </summary>
        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MilestoneStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the description of the milestone.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}