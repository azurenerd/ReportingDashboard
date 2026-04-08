using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.AspNetCore.Hosting;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private readonly IWebHostEnvironment _environment;
    private const string DataFileName = "data.json";
    private const string CacheKey = "project_data";
    private const int DefaultCacheTtlHours = 1;

    public DataProvider(IDataCache cache, ILogger<DataProvider> logger, IWebHostEnvironment environment)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to load project data...");

            var cached = await _cache.GetAsync<Project>(CacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Project data loaded successfully from cache.");
                return cached;
            }

            _logger.LogWarning("Cache miss for project data. Reading from file.");

            Project? project;
            try
            {
                var dataFilePath = GetDataFilePath();
                var json = await ReadJsonFileAsync(dataFilePath);
                _logger.LogDebug("Successfully read {ByteCount} bytes from {FilePath}", json.Length, dataFilePath);
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

    private string GetDataFilePath()
    {
        var wwwrootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(wwwrootPath))
        {
            wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }
        return Path.Combine(wwwrootPath, DataFileName);
    }

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
        }
    }

    private void ValidateProjectData(Project? project)
    {
        if (project == null)
        {
            throw new InvalidOperationException("Project data is null. Unable to deserialize project from data.json.");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            throw new InvalidOperationException("Project name is required and must not be empty or whitespace.");
        }

        if (project.Milestones == null)
        {
            throw new InvalidOperationException("Project milestones collection is null. At least one milestone is required.");
        }

        if (project.Milestones.Count == 0)
        {
            throw new InvalidOperationException("Project must have at least one milestone. The milestones array is empty.");
        }

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

        if (project.WorkItems == null)
        {
            throw new InvalidOperationException("Project work items collection is null. Work items array is required (can be empty).");
        }

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

        if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
        {
            throw new InvalidOperationException(
                $"Project completion percentage must be between 0 and 100, but got {project.CompletionPercentage}.");
        }

        if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
        {
            throw new InvalidOperationException(
                $"Project has invalid health status '{project.HealthStatus}'. " +
                $"Valid statuses are: {string.Join(", ", Enum.GetNames(typeof(HealthStatus)))}");
        }

        if (project.VelocityThisMonth < 0)
        {
            throw new InvalidOperationException(
                $"Project velocity this month must be non-negative, but got {project.VelocityThisMonth}.");
        }
    }
}