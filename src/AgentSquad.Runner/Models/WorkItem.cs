using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a work item in the project.
/// </summary>
public class WorkItem
{
    /// <summary>
    /// Initializes a new instance of the WorkItem class.
    /// </summary>
    public WorkItem() { }

    /// <summary>
    /// Gets or sets the work item title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the work item description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the work item status.
    /// </summary>
    [JsonPropertyName("status")]
    public WorkItemStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the person assigned to this work item.
    /// </summary>
    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }
}