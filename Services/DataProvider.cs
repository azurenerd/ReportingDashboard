using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for reading, parsing, validating, and caching JSON project data.
/// </summary>
public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DATA_FILE_PATH = "wwwroot/data.json";
    private const string CACHE_KEY = "project_data";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the DataProvider class.
    /// </summary>
    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads project data asynchronously from data.json.
    /// </summary>
    public async Task<Project> LoadProjectDataAsync()
    {
        _logger.LogInformation("Attempting to load project data from {DataFilePath}", DATA_FILE_PATH);

        try
        {
            // Check cache first
            var cachedProject = _cache.Get<Project>(CACHE_KEY);
            if (cachedProject != null)
            {
                _logger.LogInformation("Project data retrieved from cache");
                return cachedProject;
            }

            // Read file
            if (!File.Exists(DATA_FILE_PATH))
            {
                _logger.LogError("Data file not found at {DataFilePath}", DATA_FILE_PATH);
                throw new FileNotFoundException($"Configuration file not found: {DATA_FILE_PATH}");
            }

            // Parse JSON
            string json = await File.ReadAllTextAsync(DATA_FILE_PATH);
            var project = JsonSerializer.Deserialize<Project>(json, JsonOptions);

            // Validate
            ValidateProjectData(project);

            // Cache result
            _cache.Set(CACHE_KEY, project, TimeSpan.FromHours(1));
            _logger.LogInformation("Project data loaded successfully and cached for 1 hour");

            return project;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json: Invalid JSON syntax");
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error while reading data.json: {Message}", ex.Message);
            throw new FileNotFoundException($"Cannot read configuration file: {DATA_FILE_PATH}", ex);
        }
    }

    /// <summary>
    /// Invalidates the cached project data.
    /// </summary>
    public void InvalidateCache()
    {
        _cache.Remove(CACHE_KEY);
        _logger.LogInformation("Project data cache invalidated");
    }

    /// <summary>
    /// Validates the project data structure and required fields.
    /// </summary>
    private void ValidateProjectData(Project project)
    {
        if (project == null)
        {
            throw new InvalidOperationException("Project data cannot be null");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            throw new InvalidOperationException("Project name is required and cannot be empty");
        }

        if (project.Milestones == null || project.Milestones.Count == 0)
        {
            throw new InvalidOperationException("At least one milestone is required");
        }

        if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
        {
            throw new InvalidOperationException("Completion percentage must be between 0 and 100");
        }

        _logger.LogInformation("Project data validation passed. Project: {ProjectName}, Milestones: {MilestoneCount}, WorkItems: {WorkItemCount}",
            project.Name, project.Milestones.Count, project.WorkItems?.Count ?? 0);
    }
}