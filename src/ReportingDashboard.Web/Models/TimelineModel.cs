namespace ReportingDashboard.Web.Models;

public sealed record TimelineModel(
    DateOnly RangeStart,
    DateOnly RangeEnd,
    IReadOnlyList<MilestoneTrack> Tracks)
{
    public static TimelineModel Empty() => new(
        RangeStart: new DateOnly(DateTime.Today.Year, 1, 1),
        RangeEnd: new DateOnly(DateTime.Today.Year, 12, 31),
        Tracks: Array.Empty<MilestoneTrack>());
}