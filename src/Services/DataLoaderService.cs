using System.Text.Json;
using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service responsible for loading and deserializing data.json file into ProjectReport POCO.
/// Uses System.Text.Json with PropertyNameCaseInsensitive for flexible JSON deserialization.
/// No validation or file watching responsibility - those are handled by DataValidator and DataWatcherService.
/// </summary>
public class DataLoaderService : IDataLoaderService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataLoaderService> _logger;

    /// <summary>
    /// Initialize DataLoaderService with configuration and logging.
    /// </summary>
    /// <param name="configuration">Application configuration (read appsettings.json)</param>
    /// <param name="logger">Logger for INFO/ERROR level events</param>
    public DataLoaderService(IConfiguration configuration, ILogger<DataLoaderService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get configured data file path from appsettings.json or return default.
    /// Resolution order: AppSettings:DataPath config value, or default "./data.json"
    /// </summary>
    /// <returns>Configured data file path (relative or absolute)</returns>
    public string GetConfiguredDataPath()
    {
        var configuredPath = _configuration["AppSettings:DataPath"];
        
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            configuredPath = "data.json";
        }

        _logger.LogInformation("Configured data path: {DataPath}", configuredPath);
        return configuredPath;
    }

    /// <summary>
    /// Load and deserialize data.json file into ProjectReport POCO.
    /// </summary>
    /// <param name="dataPath">Optional file path; if not provided, uses GetConfiguredDataPath()</param>
    /// <returns>Deserialized ProjectReport object</returns>
    /// <exception cref="FileNotFoundException">Thrown if data file not found at specified path</exception>
    /// <exception cref="JsonException">Thrown if JSON is malformed or invalid</exception>
    /// <exception cref="IOException">Thrown if file cannot be read (access denied, file locked, etc.)</exception>
    public async Task<ProjectReport> LoadAsync(string dataPath = null)
    {
        // Resolve data file path: parameter > config > default
        if (string.IsNullOrWhiteSpace(dataPath))
        {
            dataPath = GetConfiguredDataPath();
        }

        _logger.LogInformation("Loading data from: {DataPath}", dataPath);

        // Validate file exists
        if (!File.Exists(dataPath))
        {
            _logger.LogError("Data file not found: {DataPath}", dataPath);
            throw new FileNotFoundException($"Data file not found at {dataPath}");
        }

        try
        {
            // Read file asynchronously
            var json = await File.ReadAllTextAsync(dataPath);
            _logger.LogInformation("File read successfully, {ByteCount} bytes", json.Length);

            // Deserialize JSON to ProjectReport POCO
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var projectReport = await Task.Run(() => 
                JsonSerializer.Deserialize<ProjectReport>(json, options)
            );

            // Log success
            _logger.LogInformation("Successfully loaded project: {ProjectName}", projectReport?.ProjectName);
            _logger.LogInformation("Loaded {MilestoneCount} milestones", projectReport?.Milestones?.Length ?? 0);

            return projectReport;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError("JSON deserialization failed: {ErrorMessage}", jsonEx.Message);
            throw;
        }
        catch (IOException ioEx)
        {
            _logger.LogError("File access error: {ErrorMessage}", ioEx.Message);
            throw;
        }
    }
}