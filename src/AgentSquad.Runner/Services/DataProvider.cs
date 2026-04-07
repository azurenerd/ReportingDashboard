using System.Text.Json;
using AgentSquad.Runner.Models;

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
            _cache = cache;
            _logger = logger;
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

                if (!File.Exists(DATA_FILE_PATH))
                {
                    throw new FileNotFoundException($"Data file not found at {DATA_FILE_PATH}");
                }

                var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var project = JsonSerializer.Deserialize<Project>(json, options);

                ValidateProjectData(project);

                await _cache.SetAsync(CACHE_KEY, project, TimeSpan.FromHours(1));
                _logger.LogInformation("Project data loaded and cached successfully");

                return project;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed");
                throw new InvalidOperationException("Invalid JSON format in data.json", ex);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Data file not found");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading project data");
                throw;
            }
        }

        public void InvalidateCache()
        {
            _cache.Remove(CACHE_KEY);
            _logger.LogInformation("Project data cache invalidated");
        }

        private void ValidateProjectData(Project? project)
        {
            if (project == null)
                throw new InvalidOperationException("Project data is null");
            if (string.IsNullOrWhiteSpace(project.Name))
                throw new InvalidOperationException("Project name is required");
            if (project.Milestones == null || project.Milestones.Count == 0)
                throw new InvalidOperationException("At least one milestone is required");
        }
    }
}