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
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Configuration file not found at: {path}");
            }

            var json = await File.ReadAllTextAsync(path);
            _cachedConfig = JsonSerializer.Deserialize<DashboardConfig>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (_cachedConfig == null)
            {
                throw new InvalidDataException("Deserialization returned null. Check that data.json contains a valid JSON object.");
            }

            if (string.IsNullOrWhiteSpace(_cachedConfig.Title))
            {
                throw new InvalidDataException("Required field 'title' is missing or empty.");
            }

            if (_cachedConfig.Months == null || _cachedConfig.Months.Count == 0)
            {
                throw new InvalidDataException("Required field 'months' is missing or empty.");
            }

            return _cachedConfig;
        }
        catch (Exception ex)
        {
            LoadError = $"Failed to load data.json: {ex.Message}";
            return null;
        }
    }
}