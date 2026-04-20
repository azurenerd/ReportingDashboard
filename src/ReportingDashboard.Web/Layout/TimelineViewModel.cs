using System.Collections.Generic;

namespace ReportingDashboard.Web.Layout;

public sealed record TimelineViewModel(
    IReadOnlyList<MonthGridline> Gridlines,
    IReadOnlyList<LaneGeometry> Lanes,
    NowMarker Now,
    int SvgWidth,
    int SvgHeight)
{
    public static TimelineViewModel Empty { get; } = new(
        new List<MonthGridline>(),
        new List<LaneGeometry>(),
        new NowMarker(0, false),
        TimelineLayoutEngine.SvgWidth,
        TimelineLayoutEngine.SvgHeight);
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
    MilestoneGeometryType Type,
    string Caption,
    CaptionPlacement CaptionPosition);

public enum MilestoneGeometryType
{
    Checkpoint,
    Poc,
    Prod
}

public enum CaptionPlacement
{
    Above,
    Below
}

public sealed record NowMarker(double X, bool InRange);