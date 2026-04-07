using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a project milestone with status and target date.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Initializes a new instance of the Milestone class.
    /// </summary>
    public Milestone() { }

    /// <summary>
    /// Gets or sets the milestone name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the milestone target date.
    /// </summary>
    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Gets or sets the milestone status.
    /// </summary>
    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the milestone description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}