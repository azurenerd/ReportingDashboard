using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents a work item tracked on the dashboard.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Gets or sets the title of the work item.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the work item.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the current status of the work item.
        /// </summary>
        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WorkItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the team or person assigned to this work item.
        /// </summary>
        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }
    }
}