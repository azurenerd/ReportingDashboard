#nullable enable

using System.Text.Json;
using AgentSquad.Runner.Exceptions;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and managing dashboard data from the data.json file.
/// Handles JSON deserialization, schema validation, and caching of configuration data.
/// </summary>
public class DashboardDataService : IDashboardDataService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<DashboardDataService> logger;
    private DashboardConfig? cachedConfig;
    private DateTime lastLoadTime = DateTime.MinValue;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<DashboardConfig> GetDashboardConfigAsync()
    {
        var dataFilePath = GetDataFilePath();

        if (cachedConfig != null && !HasFileBeenModified(dataFilePath))
        {
            logger.LogDebug("Returning cached dashboard configuration");
            return cachedConfig;
        }

        return await LoadConfigFromFileAsync(dataFilePath);
    }

    public async Task RefreshAsync()
    {
        cachedConfig = null;
        lastLoadTime = DateTime.MinValue;
        var dataFilePath = GetDataFilePath();
        await LoadConfigFromFileAsync(dataFilePath);
    }

    public DateTime GetLastModifiedTime()
    {
        var dataFilePath = GetDataFilePath();

        try
        {
            if (!File.Exists(dataFilePath))
            {
                logger.LogWarning("Data file does not exist at {Path}", dataFilePath);
                return DateTime.UtcNow;
            }

            var fileInfo = new FileInfo(dataFilePath);
            return fileInfo.LastWriteTimeUtc;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting last modified time for {Path}", dataFilePath);
            return DateTime.UtcNow;
        }
    }

    private string GetDataFilePath()
    {
        var configPath = configuration["Dashboard:DataFilePath"] 
            ?? "./wwwroot/data/data.json";

        if (!Path.IsPathRooted(configPath))
        {
            configPath = Path.Combine(AppContext.BaseDirectory, configPath);
        }

        logger.LogDebug("Data file path configured as: {Path}", configPath);
        return configPath;
    }

    private bool HasFileBeenModified(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return true;
            }

            var lastModified = File.GetLastWriteTimeUtc(filePath);
            return lastModified > lastLoadTime;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking file modification time for {Path}", filePath);
            return true;
        }
    }

    private async Task<DashboardConfig> LoadConfigFromFileAsync(string dataFilePath)
    {
        try
        {
            if (!File.Exists(dataFilePath))
            {
                throw new DashboardException(
                    $"Data file not found at '{dataFilePath}'. Please ensure the file exists and the path in appsettings.json is correct."
                );
            }

            logger.LogInformation("Loading dashboard configuration from {Path}", dataFilePath);

            var json = await File.ReadAllTextAsync(dataFilePath);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<DashboardConfig>(json, options);

                if (config == null)
                {
                    throw new Exceptions.InvalidDataException("JSON deserialization resulted in null object");
                }

                ValidateConfigSchema(config);

                cachedConfig = config;
                lastLoadTime = DateTime.UtcNow;

                logger.LogInformation("Successfully loaded dashboard configuration with {QuarterCount} quarters and {MilestoneCount} milestones",
                    config.Quarters.Count, config.Milestones.Count);

                return config;
            }
            catch (JsonException jsonEx)
            {
                throw new Exceptions.InvalidDataException($"Invalid JSON in data.json: {jsonEx.Message}", jsonEx);
            }
        }
        catch (DashboardException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error loading dashboard configuration from {Path}", dataFilePath);
            throw new DashboardException(
                $"Failed to load dashboard data from '{dataFilePath}': {ex.Message}", ex
            );
        }
    }

    private void ValidateConfigSchema(DashboardConfig config)
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(config.ProjectName))
            missingFields.Add("projectName");

        if (string.IsNullOrWhiteSpace(config.Description))
            missingFields.Add("description");

        if (config.Quarters == null || config.Quarters.Count == 0)
            missingFields.Add("quarters (must contain at least one quarter)");

        if (config.Milestones == null)
            missingFields.Add("milestones");

        if (missingFields.Count > 0)
        {
            throw new Exceptions.InvalidDataException(
                $"Dashboard data validation failed. Missing or invalid fields: {string.Join(", ", missingFields)}"
            );
        }

        foreach (var quarter in config.Quarters)
        {
            if (string.IsNullOrWhiteSpace(quarter.Month))
                missingFields.Add($"Quarter month name is empty");

            if (quarter.Year < 2000 || quarter.Year > 2099)
                missingFields.Add($"Quarter year {quarter.Year} is outside valid range (2000-2099)");
        }

        foreach (var milestone in config.Milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Id))
                missingFields.Add("Milestone id is empty");

            if (string.IsNullOrWhiteSpace(milestone.Label))
                missingFields.Add("Milestone label is empty");

            if (string.IsNullOrWhiteSpace(milestone.Date))
                missingFields.Add("Milestone date is empty");
            else if (!DateTime.TryParse(milestone.Date, out _))
                missingFields.Add($"Milestone date '{milestone.Date}' is not in valid format (expected ISO 8601: YYYY-MM-DD)");

            if (string.IsNullOrWhiteSpace(milestone.Type) || 
                !new[] { "poc", "release", "checkpoint" }.Contains(milestone.Type))
                missingFields.Add($"Milestone type '{milestone.Type}' is invalid (must be 'poc', 'release', or 'checkpoint')");
        }

        if (missingFields.Count > 0)
        {
            throw new Exceptions.InvalidDataException(
                $"Dashboard data validation failed. Invalid fields: {string.Join(", ", missingFields)}"
            );
        }

        logger.LogDebug("Dashboard configuration schema validation passed");
    }
}