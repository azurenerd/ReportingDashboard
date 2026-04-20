using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

/// <summary>
/// Stub - fleshed out by a downstream task (T5) with coordinate math for
/// NOW-line, month gridlines, lane Y distribution, and milestone positioning.
/// </summary>
public static class TimelineMath
{
    public const int SvgWidth = 1560;
    public const int SvgHeight = 185;
    public const int TopPad = 20;
    public const int BottomPad = 20;

    public static TimelineViewModel Build(
        Timeline timeline,
        DateOnly today,
        int svgWidth = SvgWidth,
        int svgHeight = SvgHeight)
    {
        _ = timeline;
        _ = today;
        _ = svgWidth;
        _ = svgHeight;
        return TimelineViewModel.Empty;
    }
}
