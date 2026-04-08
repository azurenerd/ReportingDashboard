namespace AgentSquad.Runner.Services;

using System.Text.Json;
using AgentSquad.Runner.Models;

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DataFilePath = "wwwroot/data.json";
    private const string CacheKey = "project_data";

    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        var cached = await _cache.GetAsync<Project>(CacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Retrieved project data from cache");
            return cached;
        }

        if (!File.Exists(DataFilePath))
        {
            throw new FileNotFoundException($"Data file not found at {DataFilePath}");
        }

        var json = await File.ReadAllTextAsync(DataFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var project = JsonSerializer.Deserialize<Project>(json, options);

        ValidateProjectData(project);

        await _cache.SetAsync(CacheKey, project, TimeSpan.FromHours(1));
        _logger.LogInformation("Loaded and cached project data successfully");

        return project;
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Project data cache invalidated");
    }

    private void ValidateProjectData(Project? project)
    {
        if (project == null)
        {
            throw new InvalidOperationException("Project data is null");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            throw new InvalidOperationException("Project name is required");
        }

        if (project.Milestones == null || project.Milestones.Count == 0)
        {
            throw new InvalidOperationException("At least one milestone is required");
        }

        if (project.WorkItems == null)
        {
            project.WorkItems = new List<WorkItem>();
        }
    }
}