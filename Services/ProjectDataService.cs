using System.Text.Json;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Data.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading, validating, and caching project data from JSON files.
/// Provides data access layer for the executive dashboard application.
/// </summary>
public class ProjectDataService
{
    private readonly ILogger<ProjectDataService> _logger;
    private ProjectData _cachedData;
    private DateTime _lastLoadTime;

    /// <summary>
    /// Initializes a new instance of the ProjectDataService class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic information.</param>
    public ProjectDataService(ILogger<ProjectDataService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cachedData = null;
        _lastLoadTime = DateTime.MinValue;
    }

    /// <summary>
    /// Asynchronously loads and deserializes project data from a JSON file.
    /// Validates the JSON structure, caches the result in memory, and throws
    /// DataLoadException for any file or deserialization errors.
    /// </summary>
    /// <param name="jsonFilePath">Full or relative path to the data.json file.</param>
    /// <returns>Deserialized ProjectData object containing all project information.</returns>
    /// <exception cref="DataLoadException">
    /// Thrown when file is not found, JSON is malformed, or deserialization results in null.
    /// </exception>
    public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
    {
        try
        {
            _logger.LogInformation("Loading project data from: {JsonFilePath}", jsonFilePath);

            var json = await File.ReadAllTextAsync(jsonFilePath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                throw new DataLoadException("JSON deserialization resulted in null");
            }

            _cachedData = data;
            _lastLoadTime = DateTime.UtcNow;

            _logger.LogInformation("Project data loaded successfully at {LoadTime}", _lastLoadTime);

            return data;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Data file not found at {JsonFilePath}", jsonFilePath);
            throw new DataLoadException("data.json not found in wwwroot directory", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in {JsonFilePath}", jsonFilePath);
            throw new DataLoadException($"Invalid JSON format: {ex.Message}", ex);
        }
        catch (DataLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading project data from {JsonFilePath}", jsonFilePath);
            throw new DataLoadException($"Unexpected error loading project data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates whether a JSON string contains valid project data structure
    /// with all required root properties: project, milestones, tasks, and metrics.
    /// Safely handles null and empty input by returning false.
    /// </summary>
    /// <param name="json">JSON string to validate.</param>
    /// <returns>True if JSON is valid and contains required fields; false otherwise.</returns>
    public bool ValidateJsonSchema(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("JSON schema validation failed: input is null or empty");
            return false;
        }

        try
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    _logger.LogWarning("JSON schema validation failed: root is not an object");
                    return false;
                }

                bool hasProject = root.TryGetProperty("project", out _);
                bool hasMilestones = root.TryGetProperty("milestones", out _);
                bool hasTasks = root.TryGetProperty("tasks", out _);
                bool hasMetrics = root.TryGetProperty("metrics", out _);

                bool isValid = hasProject && hasMilestones && hasTasks && hasMetrics;

                if (isValid)
                {
                    _logger.LogInformation("JSON schema validation passed");
                }
                else
                {
                    _logger.LogWarning("JSON schema validation failed: missing required fields. Project: {HasProject}, Milestones: {HasMilestones}, Tasks: {HasTasks}, Metrics: {HasMetrics}",
                        hasProject, hasMilestones, hasTasks, hasMetrics);
                }

                return isValid;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON schema validation failed: invalid JSON format");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JSON schema validation");
            return false;
        }
    }

    /// <summary>
    /// Retrieves the last-loaded project data from the in-memory cache.
    /// Returns null if no data has been loaded yet or if the cache has been cleared.
    /// </summary>
    /// <returns>Cached ProjectData object, or null if cache is empty.</returns>
    public ProjectData GetCachedData()
    {
        if (_cachedData != null)
        {
            _logger.LogInformation("Retrieving cached project data loaded at {LoadTime}", _lastLoadTime);
        }
        else
        {
            _logger.LogInformation("No cached project data available");
        }

        return _cachedData;
    }

    /// <summary>
    /// Clears the in-memory cache and resets the load timestamp to its minimum value.
    /// After calling this method, GetCachedData() will return null until new data is loaded
    /// via LoadProjectDataAsync().
    /// </summary>
    public void RefreshData()
    {
        _logger.LogInformation("Refreshing project data cache");
        _cachedData = null;
        _lastLoadTime = DateTime.MinValue;
        _logger.LogInformation("Project data cache cleared");
    }
}