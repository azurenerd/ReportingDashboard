using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents a work item tracked in the project dashboard.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Gets or sets the title or name of the work item.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional description or additional details about the work item.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the current status of this work item.
        /// </summary>
        [JsonPropertyName("status")]
        public WorkItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the team member or group assigned to this work item.
        /// </summary>
        [JsonPropertyName("assignedTo")]
        public string? AssignedTo { get; set; }
    }
}