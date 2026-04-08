using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.AspNetCore.Hosting;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and managing project data from JSON configuration file.
/// Reads from wwwroot/data.json, deserializes to Project model, validates, and caches results.
/// Includes comprehensive error handling and logging for diagnostics.
/// </summary>
public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private readonly IWebHostEnvironment _environment;
    private const string DataFileName = "data.json";
    private const string CacheKey = "project_data";
    private const int DefaultCacheTtlHours = 1;

    /// <summary>
    /// Initializes a new instance of DataProvider.
    /// </summary>
    /// <param name="cache">In-memory cache service for storing parsed project data.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="environment">Web host environment for resolving wwwroot directory.</param>
    public DataProvider(IDataCache cache, ILogger<DataProvider> logger, IWebHostEnvironment environment)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Asynchronously loads project data from wwwroot/data.json.
    /// Returns cached result if available, otherwise reads, parses, validates, and caches file.
    /// </summary>
    /// <returns>Strongly-typed Project model with nested collections.</returns>
    /// <exception cref="FileNotFoundException">Thrown when data.json is not found.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when JSON is malformed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public async Task<Project> LoadProjectDataAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to load project data...");

            // Check cache first
            var cached = await _cache.GetAsync<Project>(CacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Project data loaded successfully from cache.");
                return cached;
            }

            _logger.LogWarning("Cache miss for project data. Reading from file.");

            // Read JSON file
            Project? project;
            try
            {
                var dataFilePath = GetDataFilePath();
                var json = await ReadJsonFileAsync(dataFilePath);
                _logger.LogDebug("Successfully read {ByteCount} bytes from {FilePath}", json.Length, dataFilePath);

                // Deserialize JSON to Project model
                project = DeserializeProjectJson(json);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Data file not found. Ensure data.json exists in the wwwroot directory.");
                throw new FileNotFoundException(
                    $"Configuration file 'data.json' not found. Please create a valid data.json file in the application's wwwroot directory.",
                    DataFileName,
                    ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON from data.json. Invalid JSON syntax or format.");
                throw new JsonException(
                    $"Invalid JSON format in 'data.json'. Please ensure the file contains valid JSON. Error: {ex.Message}",
                    ex);
            }

            // Validate project data
            try
            {
                ValidateProjectData(project);
                _logger.LogInformation("Project data validation passed for project: {ProjectName}", project?.Name ?? "Unknown");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Project data validation failed. The data.json file does not contain required or valid fields.");
                throw new InvalidOperationException(
                    $"Project data validation failed: {ex.Message}. Please check the data.json file for correct structure and values.",
                    ex);
            }

            // Cache the parsed project
            await CacheProjectDataAsync(project);

            _logger.LogInformation("Project data loaded and cached successfully for project: {ProjectName}", project.Name);
            return project;
        }
        catch (Exception ex) when (!(ex is FileNotFoundException) && !(ex is JsonException) && !(ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Unexpected error occurred while loading project data.");
            throw;
        }
    }

    /// <summary>
    /// Invalidates the cached project data, forcing a reload on the next call.
    /// </summary>
    public void InvalidateCache()
    {
        try
        {
            _cache.Remove(CacheKey);
            _logger.LogInformation("Project data cache invalidated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while invalidating project data cache.");
            throw;
        }
    }

    /// <summary>
    /// Gets the full path to the data.json file using IWebHostEnvironment.
    /// </summary>
    /// <returns>Full file path to data.json.</returns>
    private string GetDataFilePath()
    {
        var wwwrootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(wwwrootPath))
        {
            wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }
        return Path.Combine(wwwrootPath, DataFileName);
    }

    /// <summary>
    /// Reads JSON content from data.json file.
    /// </summary>
    /// <param name="filePath">Full path to the data.json file.</param>
    /// <returns>JSON string content.</returns>
    /// <exception cref="FileNotFoundException">Thrown when file does not exist.</exception>
    private async Task<string> ReadJsonFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            return json;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error occurred while reading {FilePath}.", filePath);
            throw new FileNotFoundException($"Unable to read file: {filePath}. {ex.Message}", filePath, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when trying to read {FilePath}. Check file permissions.", filePath);
            throw new FileNotFoundException($"Access denied reading file: {filePath}. Check file permissions.", filePath, ex);
        }
    }

    /// <summary>
    /// Deserializes JSON string to Project model.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized Project object.</returns>
    /// <exception cref="System.Text.Json.JsonException">Thrown when JSON deserialization fails.</exception>
    private Project? DeserializeProjectJson(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new JsonException("JSON content is empty or whitespace.");
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };

            var project = JsonSerializer.Deserialize<Project>(json, jsonOptions);
            return project;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error: {Message}. Line: {LineNumber}, BytePosition: {BytePosition}",
                ex.Message, ex.LineNumber, ex.BytePosition);
            throw;
        }
    }

    /// <summary>
    /// Caches the project data with default TTL.
    /// </summary>
    /// <param name="project">Project to cache.</param>
    private async Task CacheProjectDataAsync(Project project)
    {
        try
        {
            await _cache.SetAsync(CacheKey, project, TimeSpan.FromHours(DefaultCacheTtlHours));
            _logger.LogDebug("Project data cached successfully with {CacheTtlHours} hour TTL.", DefaultCacheTtlHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching project data. Application will continue without cache.");
            // Don't throw - cache is optional for functionality
        }
    }

    /// <summary>
    /// Validates project data integrity and structure.
    /// </summary>
    /// <param name="project">Project model to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    private void ValidateProjectData(Project? project)
    {
        // Validate project is not null
        if (project == null)
        {
            throw new InvalidOperationException("Project data is null. Unable to deserialize project from data.json.");
        }

        // Validate project name
        if (string.IsNullOrWhiteSpace(project.Name))
        {
            throw new InvalidOperationException("Project name is required and must not be empty or whitespace.");
        }

        // Validate milestones collection exists and has at least one item
        if (project.Milestones == null)
        {
            throw new InvalidOperationException("Project milestones collection is null. At least one milestone is required.");
        }

        if (project.Milestones.Count == 0)
        {
            throw new InvalidOperationException("Project must have at least one milestone. The milestones array is empty.");
        }

        // Validate each milestone
        for (int i = 0; i < project.Milestones.Count; i++)
        {
            var milestone = project.Milestones[i];

            if (milestone == null)
            {
                throw new InvalidOperationException($"Milestone at index {i} is null.");
            }

            if (string.IsNullOrWhiteSpace(milestone.Name))
            {
                throw new InvalidOperationException($"Milestone at index {i} has empty or null name. All milestones must have names.");
            }

            if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
            {
                throw new InvalidOperationException(
                    $"Milestone '{milestone.Name}' has invalid status '{milestone.Status}'. " +
                    $"Valid statuses are: {string.Join(", ", Enum.GetNames(typeof(MilestoneStatus)))}");
            }
        }

        // Validate work items collection
        if (project.WorkItems == null)
        {
            throw new InvalidOperationException("Project work items collection is null. Work items array is required (can be empty).");
        }

        // Validate each work item
        for (int i = 0; i < project.WorkItems.Count; i++)
        {
            var workItem = project.WorkItems[i];

            if (workItem == null)
            {
                throw new InvalidOperationException($"Work item at index {i} is null.");
            }

            if (string.IsNullOrWhiteSpace(workItem.Title))
            {
                throw new InvalidOperationException($"Work item at index {i} has empty or null title. All work items must have titles.");
            }

            if (!Enum.IsDefined(typeof(WorkItemStatus), workItem.Status))
            {
                throw new InvalidOperationException(
                    $"Work item '{workItem.Title}' has invalid status '{workItem.Status}'. " +
                    $"Valid statuses are: {string.Join(", ", Enum.GetNames(typeof(WorkItemStatus)))}");
            }
        }

        // Validate completion percentage is in valid range (0-100)
        if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
        {
            throw new InvalidOperationException(
                $"Project completion percentage must be between 0 and 100, but got {project.CompletionPercentage}.");
        }

        // Validate health status enum value
        if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
        {
            throw new InvalidOperationException(
                $"Project has invalid health status '{project.HealthStatus}'. " +
                $"Valid statuses are: {string.Join(", ", Enum.GetNames(typeof(HealthStatus)))}");
        }

        // Validate velocity is non-negative
        if (project.VelocityThisMonth < 0)
        {
            throw new InvalidOperationException(
                $"Project velocity this month must be non-negative, but got {project.VelocityThisMonth}.");
        }
    }
}