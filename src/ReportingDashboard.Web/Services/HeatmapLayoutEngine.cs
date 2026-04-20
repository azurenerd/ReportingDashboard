using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// STUB — T5 will implement truncation and current-month resolution. Returning .Empty keeps partials renderable.
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