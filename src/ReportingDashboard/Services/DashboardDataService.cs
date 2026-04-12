using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<DashboardConfig> LoadAsync()
    {
        var path = Path.Combine(_env.WebRootPath, "data.json");

        if (!File.Exists(path))
        {
            throw new DashboardDataException(
                $"Data file not found: {path}. Ensure data.json exists in wwwroot/.");
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize<DashboardConfig>(json, options)
                ?? throw new DashboardDataException(
                    "data.json deserialized to null. Ensure the file contains valid JSON.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json");
            throw new DashboardDataException(
                $"Invalid JSON in data.json: {ex.Message}. Check for syntax errors.");
        }
    }

    public static double GetXPosition(DateOnly date, DateOnly start, DateOnly end, double totalWidth)
    {
        if (end == start) return 0;
        var fraction = (double)(date.DayNumber - start.DayNumber)
                     / (end.DayNumber - start.DayNumber);
        return Math.Clamp(fraction * totalWidth, 0, totalWidth);
    }

    public static double GetMilestoneLaneY(int index, int totalMilestones, double svgHeight)
    {
        if (totalMilestones <= 0) return svgHeight / 2;
        var spacing = svgHeight / (totalMilestones + 1);
        return spacing * (index + 1);
    }
}

public class DashboardDataException : Exception
{
    public DashboardDataException(string message) : base(message) { }
    public DashboardDataException(string message, Exception inner) : base(message, inner) { }
}