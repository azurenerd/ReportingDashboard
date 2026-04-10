namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a major project milestone in the timeline.
/// Milestones are displayed horizontally on the dashboard with status badges.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Unique identifier for the milestone (required). Example: "m001"
    /// Must be globally unique within the Milestones array.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Milestone name (required). Example: "Design Review"
    /// Displayed in the timeline visualization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional narrative description of the milestone goal or context.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Target date for milestone completion in ISO 8601 format.
    /// Used for chronological ordering in the timeline (left to right).
    /// </summary>
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Current status of the milestone (required).
    /// Valid values: "Completed", "OnTrack", "AtRisk", "Delayed"
    /// Default: "OnTrack"
    /// Validated by ProjectDashboardValidator.
    /// Maps to timeline color: green, blue, orange, red respectively.
    /// </summary>
    public string Status { get; set; } = "OnTrack";
}