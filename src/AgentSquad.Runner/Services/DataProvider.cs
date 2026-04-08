using AgentSquad.Runner.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AgentSquad.Runner.Services
{
    public interface IDataProvider
    {
        Task<Project> LoadProjectDataAsync();
        void InvalidateCache();
    }

    public class DataProvider : IDataProvider
    {
        private readonly IDataCache _cache;
        private readonly ILogger<DataProvider> _logger;
        private const string DATA_FILE_PATH = "wwwroot/data.json";
        private const string CACHE_KEY = "project_data";

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
                    _logger.LogInformation("Project data retrieved from cache");
                    return cached;
                }

                _logger.LogInformation("Loading project data from {FilePath}", DATA_FILE_PATH);

                if (!File.Exists(DATA_FILE_PATH))
                {
                    throw new FileNotFoundException($"Project data file not found at {DATA_FILE_PATH}");
                }

                var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var project = JsonSerializer.Deserialize<Project>(json, options);

                ValidateProjectData(project);

                await _cache.SetAsync(CACHE_KEY, project, TimeSpan.FromHours(1));
                _logger.LogInformation("Project data loaded and cached successfully");

                return project;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Project data file not found");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON format in project data file");
                throw new InvalidOperationException("Project data file contains invalid JSON format", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Project data validation failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading project data");
                throw new InvalidOperationException("Failed to load project data", ex);
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
                throw new InvalidOperationException("Project data cannot be null");
            }

            if (string.IsNullOrWhiteSpace(project.Name))
            {
                throw new InvalidOperationException("Project name is required and cannot be empty");
            }

            if (project.Milestones == null || project.Milestones.Count == 0)
            {
                throw new InvalidOperationException("Project must contain at least one milestone");
            }

            if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
            {
                throw new InvalidOperationException("CompletionPercentage must be between 0 and 100");
            }

            if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
            {
                throw new InvalidOperationException($"Invalid HealthStatus value: {project.HealthStatus}");
            }

            foreach (var milestone in project.Milestones)
            {
                if (milestone == null)
                {
                    throw new InvalidOperationException("Milestone cannot be null");
                }

                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    throw new InvalidOperationException("Milestone name is required and cannot be empty");
                }

                if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
                {
                    throw new InvalidOperationException($"Invalid MilestoneStatus value for milestone '{milestone.Name}': {milestone.Status}");
                }
            }

            if (project.WorkItems != null)
            {
                foreach (var workItem in project.WorkItems)
                {
                    if (workItem == null)
                    {
                        throw new InvalidOperationException("WorkItem cannot be null");
                    }

                    if (string.IsNullOrWhiteSpace(workItem.Title))
                    {
                        throw new InvalidOperationException("WorkItem title is required and cannot be empty");
                    }

                    if (!Enum.IsDefined(typeof(WorkItemStatus), workItem.Status))
                    {
                        throw new InvalidOperationException($"Invalid WorkItemStatus value for work item '{workItem.Title}': {workItem.Status}");
                    }
                }
            }
        }
    }
}