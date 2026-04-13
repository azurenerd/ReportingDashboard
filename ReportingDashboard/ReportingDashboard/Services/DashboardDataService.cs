using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

/// <summary>
/// Reads and deserializes wwwroot/data/data.json.
/// The GetDashboardConfigAsync() method body will be implemented in a subsequent PR.
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
        throw new NotImplementedException("DashboardDataService will be implemented in a subsequent PR.");
    }
}