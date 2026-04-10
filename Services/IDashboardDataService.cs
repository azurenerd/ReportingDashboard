using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDashboardDataService : IDisposable
{
    DashboardData? Data { get; }
    string? LoadError { get; }
    bool IsLoaded { get; }
    event Action? OnDataChanged;
    Task LoadAsync();
}