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
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        try
        {
            var cached = await _cache.GetAsync<Project>(CacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Project data retrieved from cache");
                return cached;
            }

            if (!File.Exists(DataFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found at {DataFilePath}");
            }

            var json = await File.ReadAllTextAsync(DataFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var project = JsonSerializer.Deserialize<Project>(json, options);

            ValidateProjectData(project);

            await _cache.SetAsync(CacheKey, project, TimeSpan.FromHours(1));
            _logger.LogInformation("Project data loaded successfully from {FilePath}", DataFilePath);

            return project;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Data file not found");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error in data file");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Project data validation failed");
            throw;
        }
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Project data cache invalidated");
    }

    private void ValidateProjectData(Project project)
    {
        if (project == null)
        {
            throw new InvalidOperationException("Project data cannot be null");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            throw new InvalidOperationException("Project name is required and cannot be empty");
        }

        if (project.Milestones == null || project.Milestones.Count == 0)
        {
            throw new InvalidOperationException("Project must have at least one milestone");
        }

        if (project.WorkItems == null)
        {
            project.WorkItems = new List<WorkItem>();
        }

        if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
        {
            throw new InvalidOperationException("Completion percentage must be between 0 and 100");
        }
    }
}