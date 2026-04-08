using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Project milestone model representing a key deliverable or phase.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Milestone name (required).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Target date for milestone completion.
    /// </summary>
    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Current status of milestone.
    /// </summary>
    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    /// <summary>
    /// Milestone description (optional).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}