namespace ReportingDashboard.Web.Models;

public sealed class DashboardData
{
    public required Project Project { get; init; }
    public required Timeline Timeline { get; init; }
    public required Heatmap Heatmap { get; init; }
    public Theme? Theme { get; init; }
}

public sealed class Theme
{
    public string? Font { get; init; }
}