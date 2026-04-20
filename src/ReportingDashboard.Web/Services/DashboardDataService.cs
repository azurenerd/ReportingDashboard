using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// Stub implementation — fleshed out by downstream task T4 (watcher, debounce,
// parsing, validation, last-known-good retention). For the scaffolding wave
// this just seeds an empty state so the app boots at http://localhost:5000.
public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    private readonly ILogger<DashboardDataService> _logger;
    private readonly object _lock = new();
    private DashboardState _current = DashboardState.Initial;

    public DashboardDataService(ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        _logger.LogInformation("DashboardDataService initialized (stub).");
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
        // Downstream task T4 will implement file read + parse + validate.
        OnChanged?.Invoke();
    }

    public void Dispose()
    {
        // Downstream task T4 will dispose FileSystemWatcher + timers here.
    }
}