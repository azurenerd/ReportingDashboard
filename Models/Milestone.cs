namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents an individual project milestone with timeline and progress tracking.
/// Immutable value object; no validation logic.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Gets the unique identifier for this milestone within the parent Milestones array.
    /// Max 50 characters, no spaces, lowercase recommended.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the milestone name/title. Non-empty, max 100 characters.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the target completion date in ISO 8601 format (YYYY-MM-DD).
    /// </summary>
    public string TargetDate { get; set; }

    /// <summary>
    /// Gets the current milestone status. Valid values: "on-track", "at-risk", "delayed", "completed".
    /// Case-sensitive, lowercase.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets the progress percentage. Integer value 0-100 inclusive.
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets the optional milestone description. Max 500 characters.
    /// </summary>
    public string Description { get; set; }
}