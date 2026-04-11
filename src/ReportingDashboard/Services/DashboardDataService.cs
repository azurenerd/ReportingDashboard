using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _dataPath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.WebRootPath, "data.json");
    }

    public async Task<DashboardLoadResult> LoadAsync()
    {
        if (!File.Exists(_dataPath))
        {
            return DashboardLoadResult.NotFound(
                "Dashboard data not found. Please ensure data.json exists in the wwwroot folder.");
        }

        try
        {
            var json = await File.ReadAllTextAsync(_dataPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            return DashboardLoadResult.Success(data!);
        }
        catch (JsonException ex)
        {
            return DashboardLoadResult.ParseError($"Error reading dashboard data: {ex.Message}");
        }
    }
}