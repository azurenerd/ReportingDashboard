using System.Text.Json;
using System.Text.Json.Serialization;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class DataLoadException : Exception
    {
        public DataLoadException(string message) : base(message) { }
        public DataLoadException(string message, Exception innerException) : base(message, innerException) { }
    }

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
                throw new DataLoadException($"Data file not found at {dataPath}");
            }

            try
            {
                var json = await File.ReadAllTextAsync(dataPath);
                ValidateJsonSchema(json);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _cachedData = JsonSerializer.Deserialize<ProjectData>(json, options);

                if (_cachedData == null)
                {
                    throw new DataLoadException("Failed to deserialize project data");
                }

                return _cachedData;
            }
            catch (JsonException ex)
            {
                throw new DataLoadException("Invalid JSON in data.json", ex);
            }
            catch (DataLoadException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DataLoadException("Unexpected error loading project data", ex);
            }
        }

        public void ValidateJsonSchema(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataLoadException("JSON content is empty or null");
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("project", out var projectElement))
                    {
                        throw new DataLoadException("Missing required 'project' object in JSON");
                    }

                    if (!root.TryGetProperty("milestones", out var milestonesElement) || milestonesElement.ValueKind != JsonValueKind.Array)
                    {
                        throw new DataLoadException("Missing required 'milestones' array in JSON");
                    }

                    if (!root.TryGetProperty("tasks", out var tasksElement) || tasksElement.ValueKind != JsonValueKind.Array)
                    {
                        throw new DataLoadException("Missing required 'tasks' array in JSON");
                    }

                    if (!root.TryGetProperty("metrics", out var metricsElement))
                    {
                        throw new DataLoadException("Missing required 'metrics' object in JSON");
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new DataLoadException("JSON schema validation failed", ex);
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