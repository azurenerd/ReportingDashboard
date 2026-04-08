using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
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
                _logger.LogInformation("Loading project data from cache or file...");

                var cached = await _cache.GetAsync<Project>(CACHE_KEY);
                if (cached != null)
                {
                    _logger.LogInformation("Project data loaded from cache");
                    return cached;
                }

                _logger.LogInformation($"Reading project data from {DATA_FILE_PATH}");
                
                if (!File.Exists(DATA_FILE_PATH))
                {
                    _logger.LogError($"Data file not found: {DATA_FILE_PATH}");
                    throw new FileNotFoundException($"Configuration file not found at {DATA_FILE_PATH}. Please ensure data.json exists in the wwwroot directory.");
                }

                var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
                _logger.LogInformation("Successfully read data.json file");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                Project project;
                try
                {
                    project = JsonSerializer.Deserialize<Project>(json, options);
                    _logger.LogInformation("Successfully deserialized JSON to Project object");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization failed");
                    throw new System.Text.Json.JsonException($"Invalid JSON format in data.json: {ex.Message}", ex);
                }

                ValidateProjectData(project);
                _logger.LogInformation("Project data validation passed");

                await _cache.SetAsync(CACHE_KEY, project, TimeSpan.FromHours(1));
                _logger.LogInformation("Project data cached for 1 hour");

                return project;
            }
            catch (FileNotFoundException)
            {
                _logger.LogError("Data file not found");
                throw;
            }
            catch (System.Text.Json.JsonException)
            {
                _logger.LogError("JSON parsing error");
                throw;
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("Project validation error");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading project data");
                throw new InvalidOperationException($"Unexpected error loading project data: {ex.Message}", ex);
            }
        }

        public void InvalidateCache()
        {
            _logger.LogInformation("Invalidating project data cache");
            _cache.Remove(CACHE_KEY);
        }

        private void ValidateProjectData(Project project)
        {
            if (project == null)
            {
                _logger.LogError("Project object is null");
                throw new InvalidOperationException("Project data is null. Ensure data.json contains valid project information.");
            }

            if (string.IsNullOrWhiteSpace(project.Name))
            {
                _logger.LogError("Project name is empty or null");
                throw new InvalidOperationException("Project name is required. Ensure the 'name' field is populated in data.json.");
            }

            if (project.Milestones == null || project.Milestones.Count == 0)
            {
                _logger.LogError("Milestones array is null or empty");
                throw new InvalidOperationException("At least one milestone is required. Ensure the 'milestones' array contains at least one item in data.json.");
            }

            if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
            {
                _logger.LogError($"Invalid completion percentage: {project.CompletionPercentage}");
                throw new InvalidOperationException("Completion percentage must be between 0 and 100.");
            }

            if (project.WorkItems == null)
            {
                project.WorkItems = new List<WorkItem>();
            }

            _logger.LogInformation($"Validation passed: Project '{project.Name}' has {project.Milestones.Count} milestones and {project.WorkItems.Count} work items");
        }
    }
}