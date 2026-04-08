using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AgentSquad.Runner.Data;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public class ProjectDataService
{
    private readonly ILogger<ProjectDataService> _logger;
    private ProjectData _cachedData;
    private DateTime _lastLoadTime;

    public ProjectDataService(ILogger<ProjectDataService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonFilePath))
            {
                throw new DataLoadException("File path cannot be null or empty");
            }

            _logger.LogInformation("Loading project data from {FilePath}", jsonFilePath);

            if (!File.Exists(jsonFilePath))
            {
                throw new DataLoadException($"data.json not found at {jsonFilePath}. Please create the file with valid project configuration.");
            }

            var json = await File.ReadAllTextAsync(jsonFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataLoadException("data.json file is empty");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                throw new DataLoadException("Failed to deserialize project data from JSON");
            }

            ValidateProjectData(data);

            _cachedData = data;
            _lastLoadTime = DateTime.Now;

            _logger.LogInformation("Project data loaded successfully. Project: {ProjectName}, Milestones: {MilestoneCount}, Tasks: {TaskCount}",
                data.Project?.Name ?? "Unknown",
                data.Milestones?.Count ?? 0,
                data.Tasks?.Count ?? 0);

            return data;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in data.json");
            throw new DataLoadException($"Invalid JSON format: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "data.json file not found");
            throw new DataLoadException($"File not found: {jsonFilePath}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error reading data.json");
            throw new DataLoadException($"Error reading file: {ex.Message}");
        }
        catch (DataLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading project data");
            throw new DataLoadException($"Unexpected error: {ex.Message}");
        }
    }

    public ProjectData GetCachedData()
    {
        if (_cachedData == null)
        {
            _logger.LogWarning("Attempted to retrieve cached data but cache is empty");
            return null;
        }

        _logger.LogInformation("Retrieved cached project data from {Time}", _lastLoadTime);
        return _cachedData;
    }

    public bool ValidateJsonSchema(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<ProjectData>(json, options);
            return data != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JSON schema validation failed");
            return false;
        }
    }

    private void ValidateProjectData(ProjectData data)
    {
        if (data.Project == null)
        {
            throw new DataLoadException("Project information is missing from data.json");
        }

        if (string.IsNullOrWhiteSpace(data.Project.Name))
        {
            throw new DataLoadException("Project name is required");
        }

        if (data.Project.StartDate == default || data.Project.EndDate == default)
        {
            throw new DataLoadException("Project start and end dates are required");
        }

        if (data.Project.StartDate >= data.Project.EndDate)
        {
            throw new DataLoadException("Project end date must be after start date");
        }

        if (data.Milestones != null)
        {
            foreach (var milestone in data.Milestones)
            {
                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    throw new DataLoadException("All milestones must have a name");
                }

                if (milestone.TargetDate == default)
                {
                    throw new DataLoadException($"Milestone '{milestone.Name}' has an invalid target date");
                }
            }
        }

        if (data.Tasks != null)
        {
            foreach (var task in data.Tasks)
            {
                if (string.IsNullOrWhiteSpace(task.Name))
                {
                    throw new DataLoadException("All tasks must have a name");
                }
            }
        }

        if (data.Metrics != null)
        {
            if (data.Metrics.TotalTasks < 0 || data.Metrics.CompletedTasks < 0)
            {
                throw new DataLoadException("Task counts cannot be negative");
            }

            if (data.Metrics.CompletedTasks > data.Metrics.TotalTasks)
            {
                throw new DataLoadException("Completed tasks cannot exceed total tasks");
            }
        }

        _logger.LogInformation("Project data validation passed");
    }
}