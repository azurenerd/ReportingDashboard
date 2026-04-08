using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class ProjectDataService
    {
        private readonly IWebHostEnvironment _environment;
        private ProjectData _cachedData;

        public ProjectDataService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ProjectData> LoadProjectDataAsync()
        {
            if (_cachedData != null)
            {
                return _cachedData;
            }

            var dataPath = Path.Combine(_environment.WebRootPath, "data", "data.json");

            if (!File.Exists(dataPath))
            {
                throw new FileNotFoundException($"Data file not found at {dataPath}");
            }

            try
            {
                var json = await File.ReadAllTextAsync(dataPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _cachedData = JsonSerializer.Deserialize<ProjectData>(json, options);

                if (_cachedData == null)
                {
                    throw new InvalidOperationException("Failed to deserialize project data");
                }

                return _cachedData;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Invalid JSON in data.json", ex);
            }
        }

        public ProjectData GetCachedData()
        {
            return _cachedData;
        }

        public void ClearCache()
        {
            _cachedData = null;
        }
    }
}