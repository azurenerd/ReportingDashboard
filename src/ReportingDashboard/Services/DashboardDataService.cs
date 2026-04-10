using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDisposable
{
    private readonly string _dataFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DashboardDataService(IWebHostEnvironment env, IConfiguration configuration)
    {
        var configuredPath = configuration["Dashboard:DataFilePath"];

        if (!string.IsNullOrEmpty(configuredPath) && Path.IsPathRooted(configuredPath))
        {
            _dataFilePath = configuredPath;
        }
        else if (!string.IsNullOrEmpty(configuredPath))
        {
            _dataFilePath = Path.Combine(env.ContentRootPath, configuredPath);
        }
        else
        {
            _dataFilePath = Path.Combine(env.WebRootPath, "data", "data.json");
        }
    }

    /// <summary>
    /// Raised when the underlying data file changes. Implementation wired in T9.
    /// </summary>
    public event Action? OnDataChanged;

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);
            return data ?? new DashboardData();
        }
        catch (FileNotFoundException)
        {
            return new DashboardData
            {
                ErrorMessage = $"data.json not found at {_dataFilePath}"
            };
        }
        catch (JsonException ex)
        {
            return new DashboardData
            {
                ErrorMessage = $"Invalid JSON in data.json: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new DashboardData
            {
                ErrorMessage = $"Unable to read data.json: {ex.Message}"
            };
        }
    }

    public void Dispose()
    {
        // FileSystemWatcher cleanup will be wired in T9
    }
}