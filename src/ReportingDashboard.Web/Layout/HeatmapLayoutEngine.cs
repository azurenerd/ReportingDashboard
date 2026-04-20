using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.ViewModels;

namespace ReportingDashboard.Web.Layout;

/// <summary>
/// Stub. Downstream task T5 will implement truncation + current-month resolution.
/// </summary>
public static class HeatmapLayoutEngine
{
    public static HeatmapViewModel Build(Heatmap heatmap, DateOnly today, int defaultMaxItems = 4)
        => HeatmapViewModel.Empty;
}