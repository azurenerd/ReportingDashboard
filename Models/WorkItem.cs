using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents the status of a work item for tracking and categorization.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkItemStatus
{
    /// <summary>
    /// Work item has been completed and shipped this month.
    /// </summary>
    Shipped = 0,

    /// <summary>
    /// Work item is currently being worked on.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Work item was not completed in the current cycle and carried over to next cycle.
    /// </summary>
    CarriedOver = 2
}

/// <summary>
/// Represents a single work item tracked in the project dashboard.
/// Work items are grouped by status (Shipped, InProgress, CarriedOver) for executive visibility.
/// </summary>
public class WorkItem
{
    /// <summary>
    /// Gets or sets the title of the work item.
    /// Brief, executive-friendly summary of the work.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the work item.
    /// Provides additional context; truncated at 100 characters in UI display.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the current status of the work item.
    /// Determines which column the item appears in on the dashboard.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WorkItemStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the name of the person or team assigned to this work item.
    /// Used for accountability and visibility.
    /// </summary>
    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }
}