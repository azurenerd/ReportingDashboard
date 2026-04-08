using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for loading and managing project data from JSON configuration file.
/// Reads from wwwroot/data.json, deserializes to Project model, and caches results.
/// </summary>
public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DataFilePath = "wwwroot/data.json";
    private const string CacheKey = "project_data";
    private const int DefaultCacheTtlHours = 1;

    /// <summary>
    /// Initializes a new instance of DataProvider.
    /// </summary>
    /// <param name="cache">In-memory cache service for storing parsed project data.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Asynchronously loads project data from wwwroot/data.json.
    /// Returns cached result if available, otherwise reads and parses file.
    /// </summary>
    /// <returns>Strongly-typed Project model with nested collections.</returns>
    public async Task<Project> LoadProjectDataAsync()
    {
        _logger.LogInformation("Attempting to load project data...");

        // Check cache first
        var cached = await _cache.GetAsync<Project>(CacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Project data loaded from cache.");
            return cached;
        }

        _logger.LogInformation("Cache miss, reading project data from file: {FilePath}", DataFilePath);

        // Read JSON file
        var json = await File.ReadAllTextAsync(DataFilePath);
        _logger.LogDebug("Read {ByteCount} bytes from {FilePath}", json.Length, DataFilePath);

        // Deserialize JSON to Project model
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        var project = JsonSerializer.Deserialize<Project>(json, jsonOptions);
        _logger.LogInformation("Successfully deserialized project data for project: {ProjectName}", project?.Name ?? "Unknown");

        // Cache the parsed project
        await _cache.SetAsync(CacheKey, project, TimeSpan.FromHours(DefaultCacheTtlHours));
        _logger.LogInformation("Project data cached for {CacheTtlHours} hour(s)", DefaultCacheTtlHours);

        return project;
    }

    /// <summary>
    /// Invalidates the cached project data, forcing a reload on the next call.
    /// </summary>
    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Project data cache invalidated.");
    }
}