using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Data.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service responsible for loading, parsing, and caching project data from JSON files.
    /// </summary>
    public class ProjectDataService
    {
        private readonly ILogger<ProjectDataService> _logger;
        private ProjectData _cachedData;
        private DateTime _lastLoadTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDataService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for diagnostic output.</param>
        public ProjectDataService(ILogger<ProjectDataService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously loads project data from a JSON file and caches the result.
        /// </summary>
        /// <param name="jsonFilePath">The file path to the project data JSON file.</param>
        /// <returns>The deserialized project data.</returns>
        /// <exception cref="DataLoadException">Thrown when the file is not found or JSON deserialization fails.</exception>
        public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(jsonFilePath);

                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<ProjectData>(json, options);

                    if (data == null)
                    {
                        _logger.LogError("JSON deserialization resulted in null for file: {FilePath}", jsonFilePath);
                        throw new DataLoadException("JSON deserialization resulted in null");
                    }

                    _cachedData = data;
                    _lastLoadTime = DateTime.UtcNow;
                    return data;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid JSON format in data file");
                    throw new DataLoadException($"Invalid JSON format: {ex.Message}");
                }
            }
            catch (FileNotFoundException)
            {
                _logger.LogError("data.json file not found at path: {FilePath}", jsonFilePath);
                throw new DataLoadException("data.json not found in wwwroot directory");
            }
        }

        /// <summary>
        /// Gets the cached project data if available.
        /// </summary>
        /// <returns>The cached project data, or null if not loaded.</returns>
        public ProjectData GetCachedData()
        {
            return _cachedData;
        }

        /// <summary>
        /// Refreshes the cached data by reloading from the file system.
        /// </summary>
        public void RefreshData()
        {
            _cachedData = null;
            _lastLoadTime = DateTime.MinValue;
        }

        /// <summary>
        /// Validates whether the provided JSON string represents valid project data structure.
        /// </summary>
        /// <param name="json">The JSON string to validate.</param>
        /// <returns>True if the JSON is valid and contains required structure; otherwise, false.</returns>
        /// <remarks>
        /// This method performs schema validation without throwing exceptions. It returns false for:
        /// - null or empty input
        /// - malformed JSON
        /// - missing required root properties (project, milestones, tasks, metrics)
        /// </remarks>
        public bool ValidateJsonSchema(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                using (var document = JsonDocument.Parse(json))
                {
                    var root = document.RootElement;

                    // Validate all required root properties are present
                    if (!root.TryGetProperty("project", out _))
                    {
                        return false;
                    }

                    if (!root.TryGetProperty("milestones", out _))
                    {
                        return false;
                    }

                    if (!root.TryGetProperty("tasks", out _))
                    {
                        return false;
                    }

                    if (!root.TryGetProperty("metrics", out _))
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}