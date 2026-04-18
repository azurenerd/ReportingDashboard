using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDashboardDataService
{
    DashboardConfig? Data { get; }
    bool IsLoaded { get; }
    string? ErrorMessage { get; }
    DateOnly NowDate { get; }
    string CurrentColumn { get; }
}