using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DATA_FILE_PATH = "wwwroot/data.json";
    private const string CACHE_KEY = "project_data";
    private static readonly TimeSpan DEFAULT_CACHE_TTL = TimeSpan.FromHours(1);

    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        try
        {
            var cached = await _cache.GetAsync<Project>(CACHE_KEY);
            if (cached != null)
            {
                _logger.LogInformation("Project data loaded from cache");
                return cached;
            }

            _logger.LogInformation("Cache miss for project data, reading from file");

            var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var project = JsonSerializer.Deserialize<Project>(json, options);

            if (project == null)
            {
                throw new InvalidOperationException("Failed to deserialize project data from JSON");
            }

            ValidateProjectData(project);

            await _cache.SetAsync(CACHE_KEY, project, DEFAULT_CACHE_TTL);
            _logger.LogInformation("Project data loaded successfully and cached");

            return project;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Data file not found at {Path}", DATA_FILE_PATH);
            throw new InvalidOperationException($"Configuration file not found at {DATA_FILE_PATH}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON from data file");
            throw new InvalidOperationException("Invalid JSON format in data file", ex);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Validation"))
        {
            _logger.LogError(ex, "Project data validation failed");
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
        {
            throw new InvalidOperationException("Validation: Project data is null");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            throw new InvalidOperationException("Validation: Project name is required and cannot be empty");
        }

        if (project.Milestones == null || project.Milestones.Count == 0)
        {
            throw new InvalidOperationException("Validation: At least one milestone is required");
        }

        if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
        {
            throw new InvalidOperationException($"Validation: CompletionPercentage must be between 0 and 100, got {project.CompletionPercentage}");
        }

        if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
        {
            throw new InvalidOperationException($"Validation: Invalid HealthStatus value '{project.HealthStatus}'");
        }

        ValidateMilestones(project.Milestones);
        ValidateWorkItems(project.WorkItems);
    }

    private void ValidateMilestones(List<Milestone> milestones)
    {
        if (milestones == null)
        {
            return;
        }

        for (int i = 0; i < milestones.Count; i++)
        {
            var milestone = milestones[i];

            if (milestone == null)
            {
                throw new InvalidOperationException($"Validation: Milestone at index {i} is null");
            }

            if (string.IsNullOrWhiteSpace(milestone.Name))
            {
                throw new InvalidOperationException($"Validation: Milestone at index {i} has an empty name");
            }

            if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
            {
                throw new InvalidOperationException($"Validation: Milestone '{milestone.Name}' has invalid status '{milestone.Status}'");
            }
        }
    }

    private void ValidateWorkItems(List<WorkItem> workItems)
    {
        if (workItems == null)
        {
            return;
        }

        for (int i = 0; i < workItems.Count; i++)
        {
            var item = workItems[i];

            if (item == null)
            {
                throw new InvalidOperationException($"Validation: WorkItem at index {i} is null");
            }

            if (string.IsNullOrWhiteSpace(item.Title))
            {
                throw new InvalidOperationException($"Validation: WorkItem at index {i} has an empty title");
            }

            if (!Enum.IsDefined(typeof(WorkItemStatus), item.Status))
            {
                throw new InvalidOperationException($"Validation: WorkItem '{item.Title}' has invalid status '{item.Status}'");
            }
        }
    }
}