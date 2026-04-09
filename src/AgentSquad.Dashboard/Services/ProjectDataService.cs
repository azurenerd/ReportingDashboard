using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Dashboard.Data;

namespace AgentSquad.Dashboard.Services;

public class DataLoadException : Exception
{
    public DataLoadException(string message) : base(message) { }
}

public class ProjectDataService
{
    private ProjectData? _cachedData;
    private DateTime _lastLoadTime = DateTime.MinValue;
    private readonly ILogger<ProjectDataService> _logger;

    public ProjectDataService(ILogger<ProjectDataService> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), jsonFilePath);

            if (!File.Exists(fullPath))
            {
                throw new DataLoadException($"data.json not found at {fullPath}");
            }

            var json = await File.ReadAllTextAsync(fullPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                throw new DataLoadException("Failed to deserialize project data: JSON structure is invalid");
            }

            ValidateProjectData(data);

            _cachedData = data;
            _lastLoadTime = DateTime.UtcNow;

            _logger.LogInformation("Successfully loaded project data from {Path}", fullPath);
            return data;
        }
        catch (JsonException ex)
        {
            throw new DataLoadException($"Invalid JSON format in data.json: {ex.Message}");
        }
        catch (IOException ex)
        {
            throw new DataLoadException($"Error reading data.json: {ex.Message}");
        }
    }

    public ProjectData? GetCachedData() => _cachedData;

    public void RefreshData()
    {
        _cachedData = null;
        _lastLoadTime = DateTime.MinValue;
    }

    private void ValidateProjectData(ProjectData data)
    {
        if (data.Project == null)
            throw new DataLoadException("Missing 'project' field in data.json");

        if (data.Milestones == null || data.Milestones.Count == 0)
            throw new DataLoadException("Missing or empty 'milestones' array in data.json");

        if (data.Tasks == null || data.Tasks.Count == 0)
            throw new DataLoadException("Missing or empty 'tasks' array in data.json");

        if (data.Metrics == null)
            throw new DataLoadException("Missing 'metrics' field in data.json");

        foreach (var milestone in data.Milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Name))
                throw new DataLoadException("Milestone must have a name");

            if (milestone.TargetDate == default)
                throw new DataLoadException($"Milestone '{milestone.Name}' has invalid targetDate");
        }

        foreach (var task in data.Tasks)
        {
            if (string.IsNullOrWhiteSpace(task.Name))
                throw new DataLoadException("Task must have a name");

            if (task.DueDate == default)
                throw new DataLoadException($"Task '{task.Name}' has invalid dueDate");
        }
    }
}