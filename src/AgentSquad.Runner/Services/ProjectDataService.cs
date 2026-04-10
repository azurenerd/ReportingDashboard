using System;
using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Validators;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public class ProjectDataService : IProjectDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProjectDataService> _logger;
    private ProjectDashboard? _cache;
    private string? _lastError;

    public bool IsInitialized => _cache != null && string.IsNullOrEmpty(_lastError);
    public string? LastError => _lastError;

    public ProjectDataService(IWebHostEnvironment env, ILogger<ProjectDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var dataPath = Path.Combine(_env.ContentRootPath, "data", "data.json");

            if (!File.Exists(dataPath))
            {
                _lastError = $"Data file not found: {dataPath}. Create the file with sample data.";
                _logger.LogError("Data file not found at {Path}", dataPath);
                throw new FileNotFoundException(_lastError, dataPath);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            using var fs = File.OpenRead(dataPath);
            var dashboard = await JsonSerializer.DeserializeAsync<ProjectDashboard>(fs, options);

            if (dashboard == null)
            {
                _lastError = "Dashboard data deserialized to null. Check data.json format.";
                _logger.LogError("Dashboard deserialization resulted in null");
                throw new InvalidOperationException(_lastError);
            }

            ProjectDashboardValidator.Validate(dashboard);

            CalculateMetrics(dashboard);

            _cache = dashboard;
            _lastError = null;

            _logger.LogInformation("✓ Dashboard data loaded successfully from {Path}", dataPath);
        }
        catch (FileNotFoundException ex)
        {
            _lastError = $"Data file not found: {ex.FileName}. Create the file with sample data.";
            _logger.LogError(ex, "File not found: {Message}", _lastError);
            throw;
        }
        catch (JsonException ex)
        {
            _lastError = $"JSON syntax error in data file: {ex.Message}. Check data.json for typos.";
            _logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _lastError = $"Data validation error: {ex.Message}";
            _logger.LogError(ex, "Data validation error: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _lastError = $"Unexpected error loading dashboard: {ex.Message}";
            _logger.LogError(ex, "Unexpected error during initialization: {Message}", ex.Message);
            throw;
        }
    }

    public ProjectDashboard GetDashboard()
    {
        if (_cache == null)
        {
            throw new InvalidOperationException("Dashboard data not initialized. Call InitializeAsync() first.");
        }
        return _cache;
    }

    public async Task RefreshAsync()
    {
        _cache = null;
        _lastError = null;
        await InitializeAsync();
    }

    private static void CalculateMetrics(ProjectDashboard dashboard)
    {
        var totalPlanned = (dashboard.Shipped?.Count ?? 0) + 
                          (dashboard.InProgress?.Count ?? 0) + 
                          (dashboard.CarriedOver?.Count ?? 0);
        var completed = dashboard.Shipped?.Count ?? 0;
        var inFlight = (dashboard.InProgress?.Count ?? 0) + (dashboard.CarriedOver?.Count ?? 0);
        var healthScore = totalPlanned > 0 ? (decimal)completed / totalPlanned * 100 : 0;

        dashboard.Metrics = new ProgressMetrics
        {
            TotalPlanned = totalPlanned,
            Completed = completed,
            InFlight = inFlight,
            HealthScore = healthScore
        };
    }
}