using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and managing project data from JSON configuration file.
/// Reads from wwwroot/data.json, deserializes to Project model, validates, and caches results.
/// </summary>
public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DataFilePath = "wwwroot/data.json";
    private const string CacheKey = "project_data";
    private const int DefaultCacheTtlHours = 1;

    /// <summary>
    /// Initializes a new instance of DataProvider.
    /// </summary>
    /// <param name="cache">In-memory cache service for storing parsed project data.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Asynchronously loads project data from wwwroot/data.json.
    /// Returns cached result if available, otherwise reads, parses, and validates file.
    /// </summary>
    /// <returns>Strongly-typed Project model with nested collections.</returns>
    /// <exception cref="FileNotFoundException">Thrown when data.json is not found.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when JSON is malformed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public async Task<Project> LoadProjectDataAsync()
    {
        _logger.LogInformation("Attempting to load project data...");

        // Check cache first
        var cached = await _cache.GetAsync<Project>(CacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Project data loaded from cache.");
            return cached;
        }

        _logger.LogInformation("Cache miss, reading project data from file: {FilePath}", DataFilePath);

        // Read JSON file
        var json = await File.ReadAllTextAsync(DataFilePath);
        _logger.LogDebug("Read {ByteCount} bytes from {FilePath}", json.Length, DataFilePath);

        // Deserialize JSON to Project model
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        var project = JsonSerializer.Deserialize<Project>(json, jsonOptions);
        _logger.LogInformation("Successfully deserialized project data for project: {ProjectName}", project?.Name ?? "Unknown");

        // Validate project data
        ValidateProjectData(project);
        _logger.LogInformation("Project data validation passed.");

        // Cache the parsed project
        await _cache.SetAsync(CacheKey, project, TimeSpan.FromHours(DefaultCacheTtlHours));
        _logger.LogInformation("Project data cached for {CacheTtlHours} hour(s)", DefaultCacheTtlHours);

        return project;
    }

    /// <summary>
    /// Invalidates the cached project data, forcing a reload on the next call.
    /// </summary>
    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Project data cache invalidated.");
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