using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DashboardDataService : IDashboardDataService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DashboardDataService> _logger;
    private DashboardConfig? _cachedConfig;
    private DateTime _lastLoadTime = DateTime.UtcNow;
    private string _dataFilePath = string.Empty;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _dataFilePath = _configuration.GetValue<string>("DashboardDataPath") ?? "./wwwroot/data/data.json";
    }

    public async Task<DashboardConfig> GetDashboardConfigAsync()
    {
        try
        {
            string fullPath = Path.Combine(AppContext.BaseDirectory, _dataFilePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"data.json not found at path: {fullPath}. " +
                    $"Please ensure the file exists and the path in appsettings.json is correct.");
            }

            string json = await File.ReadAllTextAsync(fullPath);
            _lastLoadTime = File.GetLastWriteTimeUtc(fullPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<DashboardConfig>(json, options);
            
            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize data.json");
            }

            await ValidateConfigSchema(config);
            _cachedConfig = config;

            _logger.LogInformation("Dashboard configuration loaded successfully from {Path}", fullPath);
            return config;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "data.json file not found");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in data.json");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "data.json schema validation failed");
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
        return _lastLoadTime;
    }

    private async Task ValidateConfigSchema(DashboardConfig config)
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(config.ProjectName))
            missingFields.Add("projectName");
        
        if (string.IsNullOrWhiteSpace(config.Description))
            missingFields.Add("description");
        
        if (config.Quarters == null || config.Quarters.Count == 0)
            missingFields.Add("quarters");
        
        if (config.Milestones == null)
            missingFields.Add("milestones");

        if (missingFields.Count > 0)
        {
            throw new InvalidOperationException(
                $"data.json schema validation failed. Missing or empty required fields: {string.Join(", ", missingFields)}");
        }

        foreach (var quarter in config.Quarters)
        {
            if (string.IsNullOrWhiteSpace(quarter.Month))
                throw new InvalidOperationException("Quarter must have a valid month name");
            
            if (quarter.Year < 2000 || quarter.Year > 2099)
                throw new InvalidOperationException($"Year {quarter.Year} is outside acceptable range (2000-2099)");
        }

        foreach (var milestone in config.Milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Id))
                throw new InvalidOperationException("Milestone must have an id");
            
            if (string.IsNullOrWhiteSpace(milestone.Label))
                throw new InvalidOperationException("Milestone must have a label");
            
            if (string.IsNullOrWhiteSpace(milestone.Date))
                throw new InvalidOperationException("Milestone must have a date");
            
            if (!DateTime.TryParse(milestone.Date, out _))
                throw new InvalidOperationException($"Milestone date '{milestone.Date}' is not a valid ISO 8601 date");
            
            if (string.IsNullOrWhiteSpace(milestone.Type) || 
                !new[] { "poc", "release", "checkpoint" }.Contains(milestone.Type.ToLower()))
                throw new InvalidOperationException($"Milestone type '{milestone.Type}' must be 'poc', 'release', or 'checkpoint'");
        }

        await Task.CompletedTask;
    }
}