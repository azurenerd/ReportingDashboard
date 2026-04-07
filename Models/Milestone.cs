using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a project milestone with a target date and completion status.
/// </summary>
public class Milestone
{
    /// <summary>
    /// The name of the milestone.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The target completion date for the milestone.
    /// </summary>
    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// The current status of the milestone (Completed, InProgress, AtRisk, or Future).
    /// </summary>
    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    /// <summary>
    /// A brief description of the milestone's purpose and deliverables.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Milestone"/> class.
    /// </summary>
    public Milestone()
    {
    }
}