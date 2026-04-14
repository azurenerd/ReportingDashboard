using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _dataFilePath;
    private DashboardData? _cachedData;
    private string? _error;
    private readonly object _lock = new();

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.ContentRootPath, "data.json");
        Reload();
    }

    public DashboardData? GetDashboardData() => _cachedData;

    public string? GetError() => _error;

    public void Reload()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    _error = "data.json not found. Please create a data.json file in the project root directory.";
                    _cachedData = null;
                    return;
                }

                var json = File.ReadAllText(_dataFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                _cachedData = JsonSerializer.Deserialize<DashboardData>(json, options);
                _error = null;

                if (_cachedData is null)
                {
                    _error = "data.json deserialized to null. Please verify the file contains valid JSON content.";
                }
            }
            catch (JsonException ex)
            {
                _error = $"Error parsing data.json: {ex.Message}";
                _cachedData = null;
            }
            catch (Exception ex)
            {
                _error = $"Error loading data.json: {ex.Message}";
                _cachedData = null;
            }
        }
    }
}