using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Work item model representing a unit of work tracked in dashboard.
/// </summary>
public class WorkItem
{
    /// <summary>
    /// Work item title (required).
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Work item description (optional).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Current status of work item.
    /// </summary>
    [JsonPropertyName("status")]
    public WorkItemStatus Status { get; set; }

    /// <summary>
    /// Team member or team assigned to this work item (optional).
    /// </summary>
    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }
}