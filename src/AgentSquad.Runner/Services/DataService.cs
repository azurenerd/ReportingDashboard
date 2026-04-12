using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class DataService : IDataService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DataService> _logger;

        public DataService(IWebHostEnvironment env, ILogger<DataService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<ProjectData> LoadProjectDataAsync()
        {
            try
            {
                var dataPath = Path.Combine(_env.WebRootPath, "data", "data.json");

                if (!File.Exists(dataPath))
                {
                    _logger.LogWarning("Data file not found at {DataPath}", dataPath);
                    throw new FileNotFoundException($"Data file not found: {dataPath}");
                }

                using var stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
                var projectData = await JsonSerializer.DeserializeAsync<ProjectData>(stream)
                    ?? throw new InvalidOperationException("Failed to deserialize project data");

                _logger.LogInformation("Successfully loaded project data from {DataPath}", dataPath);
                return projectData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project data");
                throw;
            }
        }
    }
}