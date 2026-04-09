namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a three-bucket categorization of delivery status:
/// Shipped (completed), InProgress (active), and CarriedOver (deferred from prior period).
/// Immutable value object; no validation logic.
/// </summary>
public class StatusSnapshot
{
    /// <summary>
    /// Gets the array of completed/shipped deliverables. Can be empty, each item max 200 characters.
    /// </summary>
    public string[] Shipped { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the array of in-progress work items. Can be empty, each item max 200 characters.
    /// </summary>
    public string[] InProgress { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the array of items carried over from the prior reporting period.
    /// Can be empty, each item max 200 characters.
    /// </summary>
    public string[] CarriedOver { get; set; } = Array.Empty<string>();
}