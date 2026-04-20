using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    private readonly object _lock = new();
    private DashboardState _current;

    public DashboardDataService()
    {
        _current = new DashboardState(
            Model: DashboardModel.Empty(),
            Error: null,
            LoadedAtUtc: DateTime.UtcNow);
    }

    public DashboardState Current
    {
        get
        {
            lock (_lock)
            {
                return _current;
            }
        }
    }

    public event Action? OnChanged;

    event Action IDashboardDataService.OnChanged
    {
        add => OnChanged += value;
        remove => OnChanged -= value;
    }

    public void Reload()
    {
        // T4 will replace this stub with FileSystemWatcher-backed reload logic.
        lock (_lock)
        {
            _current = _current with { LoadedAtUtc = DateTime.UtcNow };
        }
        OnChanged?.Invoke();
    }

    public void Dispose()
    {
        // No resources held in the stub.
    }
}