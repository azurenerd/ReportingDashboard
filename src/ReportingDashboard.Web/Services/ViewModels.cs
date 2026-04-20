using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed record MonthGridline(double X, string Label);

public sealed record MilestoneGeometry(
    double X,
    double Y,
    MilestoneType Type,
    string Caption,
    CaptionPosition CaptionPosition);

public sealed record LaneGeometry(
    string Id,
    string Label,
    string Color,
    double Y,
    IReadOnlyList<MilestoneGeometry> Milestones);

public sealed record NowMarker(double X, bool InRange);

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

public sealed record HeatmapCellView(
    IReadOnlyList<string> Items,
    int OverflowCount,
    bool IsEmpty);

public sealed record HeatmapRowView(
    HeatmapCategory Category,
    string HeaderLabel,
    IReadOnlyList<HeatmapCellView> Cells);

public sealed record HeatmapViewModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,
    IReadOnlyList<HeatmapRowView> Rows)
{
    public static HeatmapViewModel Empty { get; } = new(
        Array.Empty<string>(),
        -1,
        Array.Empty<HeatmapRowView>());
}