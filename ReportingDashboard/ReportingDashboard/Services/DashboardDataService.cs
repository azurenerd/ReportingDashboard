using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

/// <summary>
/// Reads and deserializes wwwroot/data/data.json.
/// The GetDashboardConfigAsync() method body will be fully implemented in a subsequent PR.
/// This stub returns null safely to avoid runtime crashes during prerendering.
/// </summary>
public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    public string? LoadError { get; private set; }

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public Task<DashboardConfig?> GetDashboardConfigAsync()
    {
        LoadError = "DashboardDataService is not yet implemented. Data loading will be added in a subsequent PR.";
        return Task.FromResult<DashboardConfig?>(null);
    }
}