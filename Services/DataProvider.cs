using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class DataProvider : IDataProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DataProvider> _logger;
        private Project _cachedProject;
        private DateTime _cacheTime = DateTime.MinValue;
        private const int CacheDurationMinutes = 5;

        public DataProvider(IWebHostEnvironment environment, ILogger<DataProvider> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<Project> LoadProjectDataAsync()
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

        public void InvalidateCache()
        {
            _cachedProject = null;
            _cacheTime = DateTime.MinValue;
            _logger.LogInformation("Project data cache invalidated");
        }
    }
}