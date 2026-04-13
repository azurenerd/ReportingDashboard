using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDashboardDataService
{
    Task<DashboardData> LoadDataAsync();
}

public class DashboardDataService : IDashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<DashboardData> LoadDataAsync()
    {
        var filePath = Path.Combine(_env.WebRootPath, "data.json");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("data.json not found at {Path}. Returning empty dashboard.", filePath);
            return new DashboardData();
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var data = await JsonSerializer.DeserializeAsync<DashboardData>(stream, JsonOptions);
            return data ?? new DashboardData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize data.json from {Path}", filePath);
            return new DashboardData();
        }
    }
}