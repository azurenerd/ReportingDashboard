using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Enumeration of possible milestone statuses.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneStatus
{
    /// <summary>Milestone has been completed.</summary>
    Completed,

    /// <summary>Milestone is currently in progress.</summary>
    InProgress,

    /// <summary>Milestone is at risk and may not complete on schedule.</summary>
    AtRisk,

    /// <summary>Milestone is planned for future execution.</summary>
    Future
}