using AgentSquad.Runner.Models;
using System.Text.Json;

namespace AgentSquad.Runner.Services
{
    public class DataProvider : IDataProvider
    {
        private readonly ILogger<DataProvider> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IDataCache _cache;
        private const string CacheKey = "ProjectData";
        private const int CacheTTLSeconds = 3600;

        public DataProvider(ILogger<DataProvider> logger, IWebHostEnvironment environment, IDataCache cache)
        {
            _logger = logger;
            _environment = environment;
            _cache = cache;
        }

        public async Task<Project> LoadProjectDataAsync()
        {
            var cachedProject = _cache.Get<Project>(CacheKey);
            if (cachedProject != null)
            {
                _logger.LogInformation("Project data loaded from cache");
                return cachedProject;
            }

            try
            {
                string dataPath = Path.Combine(_environment.ContentRootPath, "data.json");

                if (!File.Exists(dataPath))
                {
                    throw new FileNotFoundException($"data.json not found at {dataPath}");
                }

                string jsonContent = await File.ReadAllTextAsync(dataPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var project = JsonSerializer.Deserialize<Project>(jsonContent, options);

                if (project == null)
                {
                    throw new InvalidOperationException("Failed to deserialize project data");
                }

                _cache.Set(project, CacheKey, TimeSpan.FromSeconds(CacheTTLSeconds));

                _logger.LogInformation("Project data loaded successfully and cached for {TTL} seconds", CacheTTLSeconds);
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project data");
                throw;
            }
        }
    }
}