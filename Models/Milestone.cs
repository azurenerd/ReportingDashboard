namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a project milestone with target date and completion status.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Unique identifier for the milestone.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Milestone name or title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Target completion date for this milestone.
    /// </summary>
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Current status of the milestone.
    /// </summary>
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    /// <summary>
    /// Completion percentage for this milestone (0-100).
    /// </summary>
    public int CompletionPercentage { get; set; }
}

/// <summary>
/// Enumeration of milestone statuses.
/// </summary>
public enum MilestoneStatus
{
    /// <summary>
    /// Milestone is pending/not yet started.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Milestone is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Milestone has been completed.
    /// </summary>
    Completed = 2
}