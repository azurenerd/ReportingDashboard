using System.Text.Json;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service for loading, parsing, and caching project data from JSON configuration files.
    /// Provides in-memory caching and data validation for the dashboard.
    /// </summary>
    public class ProjectDataService
    {
        private readonly ILogger<ProjectDataService> _logger;
        private object? _cachedData;
        private DateTime _lastLoadTime = DateTime.MinValue;

        public ProjectDataService(ILogger<ProjectDataService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Loads project data from a JSON file asynchronously.
        /// Deserializes using System.Text.Json with case-insensitive property matching.
        /// Caches the result in memory for subsequent calls.
        /// </summary>
        /// <param name="jsonFilePath">Full path to the data.json file</param>
        /// <returns>Deserialized ProjectData object</returns>
        /// <exception cref="DataLoadException">Thrown when JSON is malformed, file not found, or deserialization fails</exception>
        public async Task<object?> LoadProjectDataAsync(string jsonFilePath)
        {
            try
            {
                _logger.LogInformation("Loading project data from {FilePath}", jsonFilePath);
                
                if (!File.Exists(jsonFilePath))
                {
                    throw new DataLoadException($"data.json not found at {jsonFilePath}");
                }

                var json = await File.ReadAllTextAsync(jsonFilePath);
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new DataLoadException("data.json is empty");
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<object>(json, options);

                if (data == null)
                {
                    throw new DataLoadException("JSON deserialization resulted in null");
                }

                _cachedData = data;
                _lastLoadTime = DateTime.UtcNow;
                
                _logger.LogInformation("Project data loaded successfully at {LoadTime}", _lastLoadTime);
                return data;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON format in data.json");
                throw new DataLoadException($"Invalid JSON format: {ex.Message}");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O error reading data.json");
                throw new DataLoadException($"Error reading data.json: {ex.Message}");
            }
            catch (DataLoadException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading project data");
                throw new DataLoadException($"Unexpected error loading project data: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates JSON schema by attempting deserialization without storing the result.
        /// </summary>
        /// <param name="json">JSON string to validate</param>
        /// <returns>True if JSON is valid; false otherwise</returns>
        public bool ValidateJsonSchema(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<object>(json, options);
                return data != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JSON schema validation failed");
                return false;
            }
        }

        /// <summary>
        /// Retrieves the most recently loaded and cached project data.
        /// </summary>
        /// <returns>Cached ProjectData object or null if no data has been loaded</returns>
        public object? GetCachedData()
        {
            if (_cachedData != null)
            {
                _logger.LogDebug("Retrieving cached project data (loaded at {LoadTime})", _lastLoadTime);
            }
            return _cachedData;
        }

        /// <summary>
        /// Gets the timestamp of the most recent successful data load.
        /// </summary>
        public DateTime LastLoadTime => _lastLoadTime;
    }
}