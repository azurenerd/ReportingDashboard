namespace ReportingDashboard.Web.Models;

public sealed class Timeline
{
    public required DateOnly Start { get; init; }
    public required DateOnly End { get; init; }
    public required IReadOnlyList<TimelineLane> Lanes { get; init; }
}