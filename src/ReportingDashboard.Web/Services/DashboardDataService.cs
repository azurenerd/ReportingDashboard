using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    private DashboardState _current;

    public DashboardDataService()
    {
        _current = new DashboardState(DashboardModel.Empty, Error: null, LoadedAtUtc: DateTime.UtcNow);
    }

    public DashboardState Current => _current;

    public event Action? OnChanged;

    event Action IDashboardDataService.OnChanged
    {
        add => OnChanged += value;
        remove => OnChanged -= value;
    }

    public void Reload()
    {
        _current = new DashboardState(DashboardModel.Empty, Error: null, LoadedAtUtc: DateTime.UtcNow);
        OnChanged?.Invoke();
    }

    public void Dispose()
    {
    }
}