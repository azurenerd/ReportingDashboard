using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private DashboardData? _cachedData;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        if (_cachedData is not null)
            return _cachedData;

        var filePath = Path.Combine(_env.WebRootPath, "data", "data.json");
        await using var stream = File.OpenRead(filePath);
        _cachedData = await JsonSerializer.DeserializeAsync<DashboardData>(stream, JsonOptions)
                      ?? throw new InvalidOperationException("Failed to deserialize data.json");
        return _cachedData;
    }
}