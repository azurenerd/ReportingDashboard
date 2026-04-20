namespace ReportingDashboard.Web.Models;

/// <summary>
/// A single milestone marker on a <see cref="TimelineLane"/>.
/// </summary>
public sealed class Milestone
{
    /// <summary>Calendar date of the milestone (day resolution; no time-of-day).</summary>
    public required DateOnly Date { get; init; }

    /// <summary>Visual marker type: PoC diamond, Production diamond, or checkpoint dot/circle.</summary>
    public required MilestoneType Type { get; init; }

    /// <summary>Short caption rendered near the marker (e.g., <c>"Mar 26 PoC"</c>).</summary>
    public required string Label { get; init; }

    /// <summary>
    /// Optional explicit caption placement override. When null, the layout
    /// engine auto-selects <see cref="Models.CaptionPosition.Above"/> /
    /// <see cref="Models.CaptionPosition.Below"/> to avoid overlap.
    /// </summary>
    public CaptionPosition? CaptionPosition { get; init; }
}