using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a work item tracked within the project.
/// </summary>
public class WorkItem
{
    /// <summary>
    /// The title or name of the work item.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the work item and its objectives.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The current status of the work item (Shipped, InProgress, or CarriedOver).
    /// </summary>
    [JsonPropertyName("status")]
    public WorkItemStatus Status { get; set; }

    /// <summary>
    /// The team member or team assigned to this work item.
    /// </summary>
    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkItem"/> class.
    /// </summary>
    public WorkItem()
    {
    }
}