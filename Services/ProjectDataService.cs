using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Dashboard.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Dashboard.Services;

public class ProjectDataService
{
    private readonly ILogger<ProjectDataService> _logger;
    private readonly IWebHostEnvironment _env;
    private ProjectData? _cachedData;
    private const string DataFilePath = "data/data.json";

    public ProjectDataService(ILogger<ProjectDataService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task<ProjectData> LoadProjectDataAsync()
    {
        if (_cachedData != null)
        {
            return _cachedData;
        }

        try
        {
            var fullPath = Path.Combine(_env.WebRootPath, DataFilePath);

            if (!File.Exists(fullPath))
            {
                _logger.LogError("Data file not found at {Path}", fullPath);
                throw new FileNotFoundException($"Data file not found at {fullPath}");
            }

            var json = await File.ReadAllTextAsync(fullPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                _logger.LogError("Failed to deserialize project data from {Path}", fullPath);
                throw new InvalidOperationException("Project data deserialization returned null");
            }

            ValidateProjectData(data);

            _cachedData = data;
            _logger.LogInformation("Successfully loaded project data: {ProjectName}", data.Project.Name);
            return data;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error while loading project data");
            throw new InvalidOperationException($"Invalid JSON format in data file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading project data");
            throw;
        }
    }

    private void ValidateProjectData(ProjectData data)
    {
        if (data.Project == null)
        {
            throw new InvalidOperationException("Project metadata is required in data.json");
        }

        if (string.IsNullOrWhiteSpace(data.Project.Name))
        {
            throw new InvalidOperationException("Project name is required");
        }

        if (data.Milestones == null)
        {
            throw new InvalidOperationException("Milestones array is required in data.json");
        }

        if (data.Tasks == null)
        {
            throw new InvalidOperationException("Tasks array is required in data.json");
        }

        if (data.Metrics == null)
        {
            throw new InvalidOperationException("Metrics object is required in data.json");
        }

        _logger.LogInformation("Project data validation passed: {MilestoneCount} milestones, {TaskCount} tasks", 
            data.Milestones.Count, data.Tasks.Count);
    }

    public void ClearCache()
    {
        _cachedData = null;
        _logger.LogInformation("Project data cache cleared");
    }

    public Task<ProjectData> ReloadProjectDataAsync()
    {
        ClearCache();
        return LoadProjectDataAsync();
    }
}