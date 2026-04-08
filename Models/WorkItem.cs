using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a single work item in the project dashboard.
/// </summary>
public class WorkItem
{
    /// <summary>
    /// Gets or sets the title of the work item.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the work item.
    /// Optional field that can be null.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the status of the work item.
    /// Determines which column (Shipped, InProgress, CarriedOver) the item appears in.
    /// </summary>
    [JsonPropertyName("status")]
    public required WorkItemStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the team member or team assigned to this work item.
    /// Optional field.
    /// </summary>
    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }
}

/// <summary>
/// Represents the possible status values for a work item.
/// Used to categorize work items into three dashboard columns.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkItemStatus
{
    /// <summary>
    /// Work item has been shipped this month. Displayed in first column.
    /// </summary>
    Shipped = 0,

    /// <summary>
    /// Work item is currently in progress. Displayed in second column.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Work item has been carried over from a previous period. Displayed in third column.
    /// </summary>
    CarriedOver = 2
}