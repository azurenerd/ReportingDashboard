using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public class DashboardDataService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DashboardDataService> _logger;
    private DashboardConfig? _cachedConfig;
    private DateTime _lastLoadTime = DateTime.MinValue;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DashboardConfig> GetDashboardConfigAsync()
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }

        var dataPath = _configuration.GetValue<string>("DashboardDataPath") ?? "./wwwroot/data/data.json";
        var fullPath = Path.GetFullPath(dataPath);

        _logger.LogInformation("Loading dashboard configuration from: {Path}", fullPath);

        if (!File.Exists(fullPath))
        {
            var errorMessage = $"data.json not found at path: {fullPath}. " +
                             "Please ensure the file exists and the path in appsettings.json is correct.";
            _logger.LogError(errorMessage);
            throw new FileNotFoundException(errorMessage);
        }

        try
        {
            var json = await File.ReadAllTextAsync(fullPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                WriteIndented = true
            };

            _cachedConfig = JsonSerializer.Deserialize<DashboardConfig>(json, options)
                ?? throw new InvalidOperationException("Failed to deserialize data.json");

            ValidateConfigSchema(_cachedConfig);
            _lastLoadTime = File.GetLastWriteTimeUtc(fullPath);

            _logger.LogInformation("Dashboard configuration loaded successfully");
            return _cachedConfig;
        }
        catch (JsonException ex)
        {
            var errorMessage = $"Invalid JSON in data.json. {ex.Message}";
            _logger.LogError(ex, errorMessage);
            throw new JsonException(errorMessage, ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration validation failed");
            throw;
        }
    }

    public async Task RefreshAsync()
    {
        _cachedConfig = null;
        _lastLoadTime = DateTime.MinValue;
        await GetDashboardConfigAsync();
    }

    public DateTime GetLastModifiedTime()
    {
        if (_lastLoadTime == DateTime.MinValue)
        {
            var dataPath = _configuration.GetValue<string>("DashboardDataPath") ?? "./wwwroot/data/data.json";
            var fullPath = Path.GetFullPath(dataPath);

            if (File.Exists(fullPath))
            {
                _lastLoadTime = File.GetLastWriteTimeUtc(fullPath);
            }
        }

        return _lastLoadTime;
    }

    private void ValidateConfigSchema(DashboardConfig config)
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(config.ProjectName))
            missingFields.Add(nameof(config.ProjectName));

        if (string.IsNullOrWhiteSpace(config.Description))
            missingFields.Add(nameof(config.Description));

        if (config.Quarters == null || config.Quarters.Count == 0)
            missingFields.Add(nameof(config.Quarters));

        if (config.Milestones == null)
            missingFields.Add(nameof(config.Milestones));

        if (missingFields.Count > 0)
        {
            var fieldsStr = string.Join(", ", missingFields);
            var errorMessage = $"data.json schema validation failed. Missing or invalid required fields: {fieldsStr}";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        ValidateQuarters(config.Quarters);
        ValidateMilestones(config.Milestones);
    }

    private void ValidateQuarters(List<Quarter> quarters)
    {
        var validMonths = new[] 
        { 
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        foreach (var quarter in quarters)
        {
            if (string.IsNullOrWhiteSpace(quarter.Month))
            {
                throw new InvalidOperationException("Quarter month cannot be empty");
            }

            if (!validMonths.Contains(quarter.Month))
            {
                throw new InvalidOperationException($"Invalid month '{quarter.Month}'. Must be a valid month name (January-December)");
            }

            if (quarter.Year < 2000 || quarter.Year > 2099)
            {
                throw new InvalidOperationException($"Invalid year '{quarter.Year}'. Must be between 2000 and 2099");
            }

            if (quarter.Shipped == null)
                quarter.Shipped = new List<string>();
            if (quarter.InProgress == null)
                quarter.InProgress = new List<string>();
            if (quarter.Carryover == null)
                quarter.Carryover = new List<string>();
            if (quarter.Blockers == null)
                quarter.Blockers = new List<string>();
        }
    }

    private void ValidateMilestones(List<Milestone> milestones)
    {
        var validTypes = new[] { "poc", "release", "checkpoint" };
        var seenIds = new HashSet<string>();

        foreach (var milestone in milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Id))
            {
                throw new InvalidOperationException("Milestone id cannot be empty");
            }

            if (!seenIds.Add(milestone.Id))
            {
                throw new InvalidOperationException($"Duplicate milestone id: '{milestone.Id}'");
            }

            if (string.IsNullOrWhiteSpace(milestone.Label))
            {
                throw new InvalidOperationException($"Milestone '{milestone.Id}' must have a label");
            }

            if (string.IsNullOrWhiteSpace(milestone.Date))
            {
                throw new InvalidOperationException($"Milestone '{milestone.Id}' must have a date");
            }

            if (!DateTime.TryParse(milestone.Date, out var parsedDate))
            {
                throw new InvalidOperationException(
                    $"Milestone '{milestone.Id}' has invalid date '{milestone.Date}'. Expected ISO 8601 format (YYYY-MM-DD)");
            }

            if (!validTypes.Contains(milestone.Type))
            {
                throw new InvalidOperationException(
                    $"Milestone '{milestone.Id}' has invalid type '{milestone.Type}'. Must be one of: {string.Join(", ", validTypes)}");
            }
        }
    }
}