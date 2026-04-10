namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a single work item (feature, task, bug fix, etc.) in the project.
/// Work items are distributed across three status columns: Shipped, InProgress, CarriedOver.
/// </summary>
public class WorkItem
{
    /// <summary>
    /// Globally unique identifier for the work item (required). Example: "w001"
    /// Must be unique across Shipped, InProgress, and CarriedOver lists.
    /// Enforced by ProjectDashboardValidator during deserialization.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Work item title (required). Example: "Implement user authentication"
    /// Displayed as the card heading in the status columns.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed description or narrative (max 500 characters).
    /// Displayed below the title in the card. Supports newlines for readability.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Completion date of the work item in ISO 8601 format.
    /// Used to sort items in reverse chronological order (most recent first)
    /// within each status column.
    /// </summary>
    public DateTime CompletedDate { get; set; }

    /// <summary>
    /// Optional owner/responsible team name. Example: "Backend Team"
    /// Reserved for Phase 2 UI enhancements; currently stored but not displayed.
    /// </summary>
    public string? Owner { get; set; }
}