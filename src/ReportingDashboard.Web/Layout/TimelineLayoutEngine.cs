using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.ViewModels;

namespace ReportingDashboard.Web.Layout;

// Stub - T6 will flesh out coordinate math.
public static class TimelineLayoutEngine
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