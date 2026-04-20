using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.ViewModels;

public sealed record TimelineViewModel(
    IReadOnlyList<MonthGridline> Gridlines,
    IReadOnlyList<LaneGeometry> Lanes,
    NowMarker Now)
{
    public static TimelineViewModel Empty { get; } = new(
        Array.Empty<MonthGridline>(),
        Array.Empty<LaneGeometry>(),
        new NowMarker(0, false));
}

public sealed record MonthGridline(double X, string Label);

public sealed record LaneGeometry(
    string Id,
    string Label,
    string Color,
    double Y,
    IReadOnlyList<MilestoneGeometry> Milestones);

public sealed record MilestoneGeometry(
    double X,
    double Y,
    MilestoneType Type,
    string Caption,
    CaptionPosition CaptionPosition);

public sealed record NowMarker(double X, bool InRange);