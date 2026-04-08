using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Enumeration of possible work item statuses.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkItemStatus
{
    /// <summary>Work item has been shipped this month.</summary>
    Shipped,

    /// <summary>Work item is currently in progress.</summary>
    InProgress,

    /// <summary>Work item was carried over from previous period.</summary>
    CarriedOver
}