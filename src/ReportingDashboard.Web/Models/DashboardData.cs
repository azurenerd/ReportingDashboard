namespace ReportingDashboard.Web.Models;

// Stub - T2 will flesh out with full required properties per architecture.
public sealed class DashboardData
{
    public required Project Project { get; init; }
    public required Timeline Timeline { get; init; }
    public required Heatmap Heatmap { get; init; }
    public Theme? Theme { get; init; }
}