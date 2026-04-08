using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class DataProvider : IDataProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DataProvider> _logger;
        private Project _cachedProject;
        private ProjectMetrics _cachedMetrics;
        private DateTime _cacheTime = DateTime.MinValue;
        private const int CacheDurationMinutes = 5;

        public DataProvider(IWebHostEnvironment environment, ILogger<DataProvider> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<Project> GetProjectDataAsync()
        {
            try
            {
                if (_cachedProject != null && DateTime.UtcNow.Subtract(_cacheTime).TotalMinutes < CacheDurationMinutes)
                {
                    return _cachedProject;
                }

                var dataPath = Path.Combine(_environment.WebRootPath, "data", "data.json");

                if (!File.Exists(dataPath))
                {
                    _logger.LogError($"data.json not found at {dataPath}");
                    throw new FileNotFoundException($"Project data file not found at {dataPath}");
                }

                var json = await File.ReadAllTextAsync(dataPath);
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                _cachedProject = JsonSerializer.Deserialize<Project>(json, options);
                _cacheTime = DateTime.UtcNow;

                if (_cachedProject == null)
                {
                    throw new InvalidOperationException("Failed to deserialize project data from data.json");
                }

                _logger.LogInformation($"Loaded project data: {_cachedProject.Name}");
                return _cachedProject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project data");
                throw;
            }
        }

        public async Task<ProjectMetrics> GetProjectMetricsAsync()
        {
            try
            {
                if (_cachedMetrics != null && DateTime.UtcNow.Subtract(_cacheTime).TotalMinutes < CacheDurationMinutes)
                {
                    return _cachedMetrics;
                }

                var project = await GetProjectDataAsync();

                _cachedMetrics = new ProjectMetrics
                {
                    CompletionPercentage = CalculateCompletionPercentage(project),
                    HealthStatus = DetermineHealthStatus(project),
                    VelocityThisMonth = CountItemsThisMonth(project),
                    VelocityLastMonth = CountItemsLastMonth(project)
                };

                _logger.LogInformation($"Calculated metrics - Completion: {_cachedMetrics.CompletionPercentage}%, Health: {_cachedMetrics.HealthStatus}");
                return _cachedMetrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating project metrics");
                throw;
            }
        }

        private int CalculateCompletionPercentage(Project project)
        {
            if (project?.WorkItems == null || project.WorkItems.Count == 0)
            {
                return 0;
            }

            int completedCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.ShippedThisMonth);
            return (int)((double)completedCount / project.WorkItems.Count * 100);
        }

        private HealthStatus DetermineHealthStatus(Project project)
        {
            if (project?.Milestones == null || project.Milestones.Count == 0)
            {
                return HealthStatus.OnTrack;
            }

            int atRiskCount = project.Milestones.Count(m => m.Status == MilestoneStatus.AtRisk);
            int blockedCount = project.Milestones.Count(m => m.Status == MilestoneStatus.Blocked);

            if (blockedCount > 0)
            {
                return HealthStatus.Blocked;
            }

            if (atRiskCount > 0)
            {
                return HealthStatus.AtRisk;
            }

            return HealthStatus.OnTrack;
        }

        private int CountItemsThisMonth(Project project)
        {
            if (project?.WorkItems == null)
            {
                return 0;
            }

            return project.WorkItems.Count(w => w.Status == WorkItemStatus.ShippedThisMonth);
        }

        private int CountItemsLastMonth(Project project)
        {
            if (project?.WorkItems == null)
            {
                return 0;
            }

            return project.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress);
        }
    }
}