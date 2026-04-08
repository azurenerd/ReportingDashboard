using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents the status category of a work item.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkItemStatus
    {
        /// <summary>
        /// Work item completed and shipped this month.
        /// </summary>
        Shipped,

        /// <summary>
        /// Work item currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Work item carried over from previous periods.
        /// </summary>
        CarriedOver
    }

    /// <summary>
    /// Represents a work item tracked in the project dashboard.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Gets or sets the title of the work item.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the work item. May be null or empty.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the status category of the work item.
        /// </summary>
        [JsonPropertyName("status")]
        public WorkItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the name of the team member or team assigned to this work item. May be null.
        /// </summary>
        [JsonPropertyName("assignedTo")]
        public string? AssignedTo { get; set; }
    }
}