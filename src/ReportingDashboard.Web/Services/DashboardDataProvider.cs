using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataProvider
{
    private readonly IOptionsMonitor<DashboardData> _monitor;
    private readonly ILogger<DashboardDataProvider> _logger;
    private readonly List<string> _warnings = new();

    public DashboardDataProvider(IOptionsMonitor<DashboardData> monitor, ILogger<DashboardDataProvider> logger)
    {
        _monitor = monitor;
        _logger = logger;
        _monitor.OnChange(_ => Revalidate());
        Revalidate();
    }

    public DashboardData Current => _monitor.CurrentValue;

    public IReadOnlyList<string> Warnings => _warnings;

    public IDisposable? OnChange(Action<DashboardData> listener) => _monitor.OnChange(listener);

    public DashboardData CurrentOrThrow()
    {
        Validate(Current);
        return Current;
    }

    public void Validate(DashboardData data)
    {
        if (data is null)
        {
            throw new DashboardDataException("data.json failed to bind to DashboardData.");
        }
    }

    private void Revalidate()
    {
        _warnings.Clear();
        try
        {
            Validate(_monitor.CurrentValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DashboardData validation failed.");
        }
    }
}