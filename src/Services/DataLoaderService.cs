using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Loads and deserializes the data.json file into a ProjectReport POCO object.
    /// Responsible for file I/O and JSON deserialization only; does not validate schema.
    /// </summary>
    public class DataLoaderService : IDataLoaderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataLoaderService> _logger;

        /// <summary>
        /// Initializes a new instance of the DataLoaderService class.
        /// </summary>
        /// <param name="configuration">Application configuration for reading AppSettings:DataPath.</param>
        /// <param name="logger">Logger for information and error logging during file I/O operations.</param>
        public DataLoaderService(IConfiguration configuration, ILogger<DataLoaderService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads and deserializes the data.json file into a ProjectReport object.
        /// Resolves the data file path in precedence order: parameter > configured path > default "./data.json".
        /// Reads file asynchronously and deserializes JSON with PropertyNameCaseInsensitive option.
        /// </summary>
        /// <param name="dataPath">Optional file path; if not provided, uses configured path or default "./data.json".</param>
        /// <returns>Deserialized ProjectReport object.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the data file does not exist at the specified path.</exception>
        /// <exception cref="JsonException">Thrown if the JSON is malformed or cannot be deserialized.</exception>
        /// <exception cref="IOException">Thrown if the file cannot be read due to access denied or file lock.</exception>
        public async Task<ProjectReport> LoadAsync(string dataPath = null)
        {
            // Resolve dataPath precedence: parameter > config > default
            if (string.IsNullOrEmpty(dataPath))
            {
                dataPath = GetConfiguredDataPath();
            }

            // Log the data path being loaded
            _logger.LogInformation($"Loading data from: {dataPath}");

            // Validate file exists
            if (!File.Exists(dataPath))
            {
                _logger.LogError($"Data file not found: {dataPath}");
                throw new FileNotFoundException($"Data file not found at {dataPath}");
            }

            try
            {
                // Read file content asynchronously
                var json = await File.ReadAllTextAsync(dataPath);
                _logger.LogInformation($"File read successfully, {json.Length} bytes");

                // Deserialize JSON with PropertyNameCaseInsensitive to handle camelCase JSON properties
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var projectReport = await Task.Run(() =>
                    JsonSerializer.Deserialize<ProjectReport>(json, options)
                );

                return projectReport;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                throw;
            }
            catch (IOException ex)
            {
                _logger.LogError($"File access error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the configured data file path from appsettings.json or returns the default.
        /// Resolves configuration via IConfiguration["AppSettings:DataPath"] and logs the result.
        /// </summary>
        /// <returns>The data file path (from configuration or default "./data.json").</returns>
        public string GetConfiguredDataPath()
        {
            var configuredPath = _configuration["AppSettings:DataPath"];

            if (string.IsNullOrEmpty(configuredPath))
            {
                configuredPath = "./data.json";
            }

            _logger.LogInformation($"Configured data path: {configuredPath}");

            return configuredPath;
        }
    }
}