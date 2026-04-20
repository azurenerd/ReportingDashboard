namespace ReportingDashboard.Web.Models;

/// <summary>
/// A single swim lane on the milestone timeline (e.g., "M1 - Chatbot &amp; MS Role").
/// </summary>
public sealed class TimelineLane
{
    /// <summary>Short lane identifier rendered bold in the left column (e.g., <c>"M1"</c>).</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable lane label rendered below the id.</summary>
    public required string Label { get; init; }

    /// <summary>Brand color for the lane track and id label. Must match <c>^#[0-9A-Fa-f]{6}$</c>.</summary>
    public required string Color { get; init; }

    /// <summary>Zero or more milestones placed on the lane. Each date must fall within the timeline range.</summary>
    public required IReadOnlyList<Milestone> Milestones { get; init; }
}