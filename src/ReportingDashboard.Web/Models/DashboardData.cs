namespace ReportingDashboard.Web.Models;

public sealed record DashboardData
{
    public ProjectInfo Project { get; init; } = new();
    public TimelineConfig Timeline { get; init; } = new();
    public HeatmapConfig Heatmap { get; init; } = new();
}