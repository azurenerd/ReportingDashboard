namespace ReportingDashboard.Web.Models;

public sealed record TimelineModel(
    DateOnly RangeStart,
    DateOnly RangeEnd,
    IReadOnlyList<MilestoneTrack> Tracks)
{
    public static TimelineModel Empty { get; } = new(
        RangeStart: new DateOnly(2026, 1, 1),
        RangeEnd: new DateOnly(2026, 6, 30),
        Tracks: Array.Empty<MilestoneTrack>());
}