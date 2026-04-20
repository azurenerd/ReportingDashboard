namespace ReportingDashboard.Web.Models;

// TODO(T2): populate full shape per architecture (Project/Timeline/Heatmap/Theme with required init props).
public sealed class DashboardData
{
    public Project? Project { get; init; }
    public Timeline? Timeline { get; init; }
    public Heatmap? Heatmap { get; init; }
}