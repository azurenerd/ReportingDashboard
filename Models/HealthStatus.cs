using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Enumeration of possible project health statuses.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthStatus
{
    /// <summary>Project is on track with no major issues.</summary>
    OnTrack,

    /// <summary>Project is at risk and may not meet objectives.</summary>
    AtRisk,

    /// <summary>Project is blocked and cannot proceed.</summary>
    Blocked
}