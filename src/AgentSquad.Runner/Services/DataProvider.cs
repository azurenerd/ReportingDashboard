using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DATA_FILE_PATH = "wwwroot/data.json";
    private const string CACHE_KEY = "project_data";

    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        // Check cache first
        var cached = await _cache.GetAsync<Project>(CACHE_KEY);
        if (cached != null)
        {
            _logger.LogInformation("Returning cached project data");
            return cached;
        }

        try
        {
            // Read and parse JSON
            if (!File.Exists(DATA_FILE_PATH))
            {
                throw new FileNotFoundException($"Configuration file not found at {DATA_FILE_PATH}");
            }

            var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var project = JsonSerializer.Deserialize<Project>(json, options);

            // Validate structure
            ValidateProjectData(project);

            // Cache result for 1 hour
            await _cache.SetAsync(CACHE_KEY, project, TimeSpan.FromHours(1));

            _logger.LogInformation("Project data loaded and cached successfully");
            return project;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError($"File error: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON parsing error: {ex.Message}");
            throw new InvalidOperationException("Invalid JSON format in data.json. Please check the file syntax.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error loading project data: {ex.Message}");
            throw;
        }
    }

    public void InvalidateCache()
    {
        _cache.Remove(CACHE_KEY);
        _logger.LogInformation("Project data cache invalidated");
    }

    private void ValidateProjectData(Project project)
    {
        if (project == null)
            throw new InvalidOperationException("Project data is null");

        if (string.IsNullOrWhiteSpace(project.Name))
            throw new InvalidOperationException("Project name is required and cannot be empty");

        if (project.Milestones == null || project.Milestones.Count == 0)
            throw new InvalidOperationException("At least one milestone is required");

        if (project.WorkItems == null)
            project.WorkItems = new List<WorkItem>();

        // Validate milestone structure
        foreach (var milestone in project.Milestones)
        {
            if (string.IsNullOrWhiteSpace(milestone.Name))
                throw new InvalidOperationException("All milestones must have a name");

            if (milestone.TargetDate == default)
                throw new InvalidOperationException("All milestones must have a valid target date");

            if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
                throw new InvalidOperationException($"Invalid milestone status: {milestone.Status}");
        }

        // Validate work item structure
        foreach (var item in project.WorkItems)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
                throw new InvalidOperationException("All work items must have a title");

            if (!Enum.IsDefined(typeof(WorkItemStatus), item.Status))
                throw new InvalidOperationException($"Invalid work item status: {item.Status}");
        }

        // Validate completion percentage
        if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
            throw new InvalidOperationException("CompletionPercentage must be between 0 and 100");

        _logger.LogInformation($"Project data validation passed for project: {project.Name}");
    }
}