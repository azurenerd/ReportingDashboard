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
            if (_cachedData?.Project != null)
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
                var data = JsonSerializer.Deserialize<ProjectData>(json, options);

                ValidateDeserializedData(data);
                _cachedData = data;

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

        public void ValidateDeserializedData(ProjectData data)
        {
            if (data == null)
            {
                throw new DataLoadException("Deserialized project data is null");
            }

            if (data.Project == null)
            {
                throw new DataLoadException("Project object is null in deserialized data");
            }

            if (string.IsNullOrWhiteSpace(data.Project.Name))
            {
                throw new DataLoadException("Project name is null or empty");
            }

            if (data.Milestones == null)
            {
                throw new DataLoadException("Milestones collection is null");
            }

            if (data.Tasks == null)
            {
                throw new DataLoadException("Tasks collection is null");
            }

            if (data.Metrics == null)
            {
                throw new DataLoadException("Metrics object is null");
            }

            if (data.Milestones.Any(m => string.IsNullOrWhiteSpace(m.Id) || string.IsNullOrWhiteSpace(m.Name)))
            {
                throw new DataLoadException("One or more milestones have missing Id or Name");
            }

            if (data.Tasks.Any(t => string.IsNullOrWhiteSpace(t.Id) || string.IsNullOrWhiteSpace(t.Name)))
            {
                throw new DataLoadException("One or more tasks have missing Id or Name");
            }

            if (data.Metrics.TotalTasks < 0 || data.Metrics.CompletedTasks < 0 || data.Metrics.InProgressTasks < 0 || data.Metrics.CarriedOverTasks < 0)
            {
                throw new DataLoadException("Metrics contain negative values");
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