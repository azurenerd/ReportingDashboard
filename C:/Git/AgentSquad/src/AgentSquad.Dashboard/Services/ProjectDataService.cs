using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentSquad.Dashboard.Services;

public class ProjectDataService
{
    private readonly ILogger<ProjectDataService> _logger;
    private ProjectData? _cachedData;
    private DateTime _lastLoadTime;

    public ProjectDataService(ILogger<ProjectDataService> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new DataLoadException($"data.json not found at {jsonFilePath}");
            }

            var json = await File.ReadAllTextAsync(jsonFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                throw new DataLoadException("JSON deserialization resulted in null object");
            }

            ValidateProjectData(data);
            _cachedData = data;
            _lastLoadTime = DateTime.UtcNow;
            return data;
        }
        catch (JsonException ex)
        {
            throw new DataLoadException($"Invalid JSON format: {ex.Message}", ex);
        }
        catch (FileNotFoundException ex)
        {
            throw new DataLoadException($"data.json not found: {ex.Message}", ex);
        }
    }

    public ProjectData GetCachedData()
    {
        return _cachedData ?? throw new InvalidOperationException("No cached data available. Call LoadProjectDataAsync first.");
    }

    private void ValidateProjectData(ProjectData data)
    {
        if (data.Project == null)
            throw new DataLoadException("Missing 'project' field in data.json");
        
        if (string.IsNullOrWhiteSpace(data.Project.Name))
            throw new DataLoadException("Project name is required");
        
        if (data.Milestones == null)
            throw new DataLoadException("Missing 'milestones' array in data.json");
        
        if (data.Tasks == null)
            throw new DataLoadException("Missing 'tasks' array in data.json");
        
        if (data.Metrics == null)
            throw new DataLoadException("Missing 'metrics' object in data.json");
    }
}

public class DataLoadException : Exception
{
    public DataLoadException(string message) : base(message) { }
    public DataLoadException(string message, Exception innerException) : base(message, innerException) { }
}

public class ProjectData
{
    [JsonPropertyName("project")]
    public ProjectInfo? Project { get; set; }

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("tasks")]
    public List<Task> Tasks { get; set; } = new();

    [JsonPropertyName("metrics")]
    public ProjectMetrics? Metrics { get; set; }
}

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "OnTrack";

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; set; } = string.Empty;

    [JsonPropertyName("projectManager")]
    public string ProjectManager { get; set; } = string.Empty;
}

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    [JsonPropertyName("actualDate")]
    public DateTime? ActualDate { get; set; }

    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }
}

public enum MilestoneStatus
{
    Completed = 0,
    InProgress = 1,
    Pending = 2
}

public class Task
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TaskStatus Status { get; set; }

    [JsonPropertyName("assignedTo")]
    public string AssignedTo { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }

    [JsonPropertyName("estimatedDays")]
    public int EstimatedDays { get; set; }

    [JsonPropertyName("relatedMilestone")]
    public string RelatedMilestone { get; set; } = string.Empty;
}

public enum TaskStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}

public class ProjectMetrics
{
    [JsonPropertyName("totalTasks")]
    public int TotalTasks { get; set; }

    [JsonPropertyName("completedTasks")]
    public int CompletedTasks { get; set; }

    [JsonPropertyName("inProgressTasks")]
    public int InProgressTasks { get; set; }

    [JsonPropertyName("carriedOverTasks")]
    public int CarriedOverTasks { get; set; }

    [JsonPropertyName("estimatedBurndownRate")]
    public double EstimatedBurndownRate { get; set; }

    public int CompletionPercentage =>
        TotalTasks > 0 ? (CompletedTasks * 100) / TotalTasks : 0;
}