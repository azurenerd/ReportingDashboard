using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    public DashboardData? Data { get; }
    public bool HasError { get; }
    public string? ErrorMessage { get; }

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "dashboard-data.json");
        try
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            Data = JsonSerializer.Deserialize<DashboardData>(json, options);
        }
        catch (FileNotFoundException)
        {
            ErrorMessage = $"Configuration file not found: {path}";
            HasError = true;
            logger.LogError(ErrorMessage);
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"JSON parse error in {path}: {ex.Message}";
            HasError = true;
            logger.LogError(ErrorMessage);
        }
    }
}
