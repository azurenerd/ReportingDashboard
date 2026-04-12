using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private DashboardConfig? _cachedConfig;
    public string? LoadError { get; private set; }

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<DashboardConfig?> GetDashboardConfigAsync()
    {
        if (_cachedConfig != null) return _cachedConfig;

        var path = Path.Combine(_env.WebRootPath, "data", "data.json");
        try
        {
            var json = await File.ReadAllTextAsync(path);
            _cachedConfig = JsonSerializer.Deserialize<DashboardConfig>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (_cachedConfig?.Title == null || _cachedConfig?.Months == null)
                throw new InvalidDataException("Required fields 'title' and 'months' are missing.");

            return _cachedConfig;
        }
        catch (Exception ex)
        {
            LoadError = $"Failed to load data.json: {ex.Message}";
            return null;
        }
    }
}