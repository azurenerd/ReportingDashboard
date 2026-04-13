using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class DashboardDataService : IDashboardDataService
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

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var filePath = Path.Combine(_env.ContentRootPath, "Data", "data.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Dashboard data file not found at expected path: {filePath}. " +
                "Ensure data.json exists in the Data/ directory at the project root.",
                filePath);
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            var data = await JsonSerializer.DeserializeAsync<DashboardData>(stream, JsonOptions);

            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize dashboard data from {filePath}. " +
                    "The file may be empty or contain invalid JSON structure.");
            }

            return data;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse dashboard data from {FilePath}", filePath);
            throw new InvalidOperationException(
                $"Failed to parse dashboard data from {filePath}: {ex.Message}", ex);
        }
    }

    public async Task<List<WorkItem>> GetWorkItemsByCategoryAsync(string category)
    {
        var data = await GetDashboardDataAsync();
        return data.WorkItems
            .Where(w => w.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}