using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _dataFilePath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.WebRootPath, "data.json");
    }

    public (DashboardData? Data, string? Error) LoadDashboard()
    {
        if (!File.Exists(_dataFilePath))
            return (null, "data.json not found. Please place a valid data.json file in the wwwroot/ directory. See data.sample.json for the expected format.");

        try
        {
            var json = File.ReadAllText(_dataFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            return (data, null);
        }
        catch (JsonException ex)
        {
            return (null, $"Unable to load dashboard data. Please check data.json for syntax errors.\n{ex.Message}");
        }
    }
}