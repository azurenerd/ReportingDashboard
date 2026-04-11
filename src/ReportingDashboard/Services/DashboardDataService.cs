using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public (DashboardData? Data, string? Error) Load()
    {
        try
        {
            var path = Path.Combine(_env.ContentRootPath, "Data", "data.json");

            if (!File.Exists(path))
            {
                return (null, $"Data file not found: {path}. Create Data/data.json to get started.");
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                return (null, "data.json deserialized to null.");
            }

            if (string.IsNullOrWhiteSpace(data.Project.Title))
            {
                return (null, "Missing required field 'project.title' in data.json.");
            }

            return (data, null);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json");
            return (null, $"Invalid JSON in data.json: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading data.json");
            return (null, $"Error loading data.json: {ex.Message}");
        }
    }
}