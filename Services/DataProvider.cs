using AgentSquad.Runner.Models;
using System.Text.Json;

namespace AgentSquad.Runner.Services
{
    public class DataProvider : IDataProvider
    {
        private readonly ILogger<DataProvider> _logger;
        private readonly IWebHostEnvironment _environment;
        private Project _cachedProject;

        public DataProvider(ILogger<DataProvider> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async Task<Project> LoadProjectDataAsync()
        {
            if (_cachedProject != null)
            {
                return _cachedProject;
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
                _cachedProject = JsonSerializer.Deserialize<Project>(jsonContent, options);

                if (_cachedProject == null)
                {
                    throw new InvalidOperationException("Failed to deserialize project data");
                }

                _logger.LogInformation("Project data loaded successfully");
                return _cachedProject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project data");
                throw;
            }
        }
    }
}