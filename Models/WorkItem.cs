using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents a work item (task, story, or epic) tracked as part of project progress.
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
        /// Gets or sets the team member or group assigned to this work item.
        /// </summary>
        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItem"/> class.
        /// </summary>
        public WorkItem()
        {
            Title = string.Empty;
            Description = string.Empty;
            AssignedTo = string.Empty;
        }
    }
}