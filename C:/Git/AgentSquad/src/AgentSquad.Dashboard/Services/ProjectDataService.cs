using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Services;

public class ProjectDataService
{
    private readonly ILogger<ProjectDataService> _logger;
    private readonly IWebHostEnvironment _env;
    private ProjectData? _cachedData;
    private DateTime _lastLoadTime;

    public ProjectDataService(ILogger<ProjectDataService> logger, IWebHostEnvironment env)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public async Task<ProjectData> LoadProjectDataAsync(string jsonFileName = "data/data.json")
    {
        try
        {
            var filePath = Path.Combine(_env.WebRootPath, jsonFileName);

            if (!File.Exists(filePath))
            {
                throw new DataLoadException($"data.json not found at {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataLoadException("data.json is empty");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                throw new DataLoadException("JSON deserialization resulted in null object");
            }

            ValidateProjectData(data);
            _cachedData = data;
            _lastLoadTime = DateTime.UtcNow;

            _logger.LogInformation("Project data loaded successfully from {Path}", filePath);
            return data;
        }
        catch (JsonException ex)
        {
            throw new DataLoadException($"Invalid JSON format in data.json: {ex.Message}", ex);
        }
        catch (FileNotFoundException ex)
        {
            throw new DataLoadException($"data.json file not found: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new DataLoadException($"Error reading data.json: {ex.Message}", ex);
        }
    }

    public ProjectData? GetCachedData() => _cachedData;

    public DateTime GetLastLoadTime() => _lastLoadTime;

    public void RefreshData()
    {
        _cachedData = null;
        _lastLoadTime = DateTime.MinValue;
    }

    private void ValidateProjectData(ProjectData data)
    {
        if (data.Project == null)
            throw new DataLoadException("Missing 'project' object in data.json");

        if (string.IsNullOrWhiteSpace(data.Project.Name))
            throw new DataLoadException("Project name is required");

        if (data.Milestones == null)
            throw new DataLoadException("Missing 'milestones' array in data.json");

        if (data.Tasks == null)
            throw new DataLoadException("Missing 'tasks' array in data.json");

        if (data.Metrics == null)
            throw new DataLoadException("Missing 'metrics' object in data.json");

        if (data.Project.StartDate > data.Project.EndDate)
            throw new DataLoadException("Project startDate must be before endDate");

        foreach (var milestone in data.Milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Name))
                throw new DataLoadException("Milestone name is required");
        }

        foreach (var task in data.Tasks)
        {
            if (string.IsNullOrWhiteSpace(task.Name))
                throw new DataLoadException("Task name is required");
        }
    }
}

public class DataLoadException : Exception
{
    public DataLoadException(string message) : base(message) { }
    public DataLoadException(string message, Exception innerException) : base(message, innerException) { }
}