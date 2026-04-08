using System.Text.Json;
using AgentSquad.Runner.Services.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and managing project data from JSON configuration file.
/// Thread-safe for Singleton registration. Stateless after initialization.
/// </summary>
public class ProjectDataService
{
    private readonly string _dataFilePath;
    private readonly ILogger<ProjectDataService> _logger;

    /// <summary>
    /// Initializes a new instance of ProjectDataService.
    /// </summary>
    /// <param name="logger">Logger for diagnostic and error messages</param>
    /// <param name="dataFilePath">Optional custom path to data.json; defaults to wwwroot/data/data.json</param>
    public ProjectDataService(ILogger<ProjectDataService> logger, string? dataFilePath = null)
    {
        _logger = logger;
        _dataFilePath = dataFilePath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot", "data", "data.json");
    }

    /// <summary>
    /// Loads project data from JSON file asynchronously.
    /// </summary>
    /// <returns>ProjectData instance; returns empty ProjectData if file not found or parsing fails</returns>
    public async Task<ProjectData> LoadProjectDataAsync()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                _logger.LogWarning("Data file not found at path: {DataFilePath}", _dataFilePath);
                return new ProjectData();
            }

            string jsonContent = await File.ReadAllTextAsync(_dataFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var projectData = JsonSerializer.Deserialize<ProjectData>(jsonContent, options);

            return projectData ?? new ProjectData();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to parse data.json at {DataFilePath}: {ErrorMessage}", _dataFilePath, jsonEx.Message);
            return new ProjectData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading project data from {DataFilePath}: {ErrorMessage}", _dataFilePath, ex.Message);
            return new ProjectData();
        }
    }

    /// <summary>
    /// Gets task count summary by status.
    /// </summary>
    public TaskStatusSummary GetTaskStatusSummary(ProjectData projectData)
    {
        return new TaskStatusSummary
        {
            ShippedCount = projectData.Tasks.Count(t => t.Status == TaskStatus.Shipped),
            InProgressCount = projectData.Tasks.Count(t => t.Status == TaskStatus.InProgress),
            CarriedOverCount = projectData.Tasks.Count(t => t.Status == TaskStatus.CarriedOver)
        };
    }
}

/// <summary>
/// Summary of task counts by status category.
/// </summary>
public class TaskStatusSummary
{
    public int ShippedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CarriedOverCount { get; set; }
}