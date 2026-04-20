namespace ReportingDashboard.Web.Models;

/// <summary>
/// Gantt-style milestone timeline: start/end date range plus 1-6 swim lanes.
/// </summary>
public sealed class Timeline
{
    /// <summary>Inclusive start date of the timeline x-axis.</summary>
    public required DateOnly Start { get; init; }

    /// <summary>Inclusive end date of the timeline x-axis. Must be &gt; <see cref="Start"/>.</summary>
    public required DateOnly End { get; init; }

    /// <summary>Swim lanes rendered top-to-bottom. Count must be 1..6.</summary>
    public required IReadOnlyList<TimelineLane> Lanes { get; init; }
}