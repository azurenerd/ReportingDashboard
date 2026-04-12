using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Implementation of IDashboardDataService for loading dashboard configuration from data.json
/// </summary>
public class DashboardDataService : IDashboardDataService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DashboardDataService> _logger;
    private DashboardConfig? _cachedConfig;
    private DateTime _lastLoadTime;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DashboardConfig> GetDashboardConfigAsync()
    {
        try
        {
            var dataPath = _configuration["DashboardDataPath"] ?? "./wwwroot/data/data.json";
            
            if (!File.Exists(dataPath))
            {
                throw new FileNotFoundException(
                    $"data.json not found at path: {dataPath}. " +
                    "Please ensure the file exists and the path in appsettings.json is correct.");
            }

            var json = await File.ReadAllTextAsync(dataPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            var config = JsonSerializer.Deserialize<DashboardConfig>(json, options);
            
            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize dashboard configuration.");
            }

            _cachedConfig = config;
            _lastLoadTime = DateTime.UtcNow;
            
            return config;
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Invalid JSON in data.json: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard configuration");
            throw;
        }
    }

    public async Task RefreshAsync()
    {
        _cachedConfig = null;
        await GetDashboardConfigAsync();
    }

    public DateTime GetLastModifiedTime()
    {
        try
        {
            var dataPath = _configuration["DashboardDataPath"] ?? "./wwwroot/data/data.json";
            
            if (!File.Exists(dataPath))
            {
                return DateTime.MinValue;
            }

            var fileInfo = new FileInfo(dataPath);
            return fileInfo.LastWriteTimeUtc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file modification time");
            return DateTime.MinValue;
        }
    }
}