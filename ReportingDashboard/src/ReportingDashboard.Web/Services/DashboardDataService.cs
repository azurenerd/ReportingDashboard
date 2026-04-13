using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class DashboardDataService : IDashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;
    private DashboardData? _cachedData;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        if (_cachedData is not null)
            return _cachedData;

        var filePath = Path.Combine(_env.WebRootPath, "data", "data.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Dashboard data file not found at: {filePath}");

        _logger.LogInformation("Loading dashboard data from {Path}", filePath);

        await using var stream = File.OpenRead(filePath);
        var data = await JsonSerializer.DeserializeAsync<DashboardData>(stream);

        _cachedData = data ?? throw new InvalidOperationException("Failed to deserialize dashboard data.");
        return _cachedData;
    }
}