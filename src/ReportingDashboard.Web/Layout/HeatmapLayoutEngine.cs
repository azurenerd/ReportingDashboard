using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.ViewModels;

namespace ReportingDashboard.Web.Layout;

// Stub - T7 will flesh out row normalization + truncation.
public static class HeatmapLayoutEngine
{
    public static HeatmapViewModel Build(
        Heatmap heatmap,
        DateOnly today,
        int defaultMaxItems = 4)
    {
        _ = heatmap;
        _ = today;
        _ = defaultMaxItems;
        return HeatmapViewModel.Empty;
    }
}