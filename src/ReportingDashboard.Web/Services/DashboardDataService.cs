using Microsoft.Extensions.Logging;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IHostEnvironment _env;
    private DashboardState _current = DashboardState.Empty;

    public DashboardDataService(IHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
        _logger.LogInformation("DashboardDataService initialized (stub); ContentRoot={Root}", _env.ContentRootPath);
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
        // Stub implementation - real loading lives in a downstream task.
        OnChanged?.Invoke();
    }

    public void Dispose()
    {
        // Nothing to dispose in the stub.
    }
}