using System.Text.Json;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DATA_FILE_PATH = "wwwroot/data.json";
    private const string CACHE_KEY = "project_data";
    private static readonly TimeSpan DEFAULT_CACHE_TTL = TimeSpan.FromHours(1);

    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        try
        {
            var cached = await _cache.GetAsync<Project>(CACHE_KEY);
            if (cached != null)
            {
                _logger.LogInformation("Project data loaded from cache");
                return cached;
            }

            _logger.LogInformation("Cache miss for project data, reading from file");

            var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var project = JsonSerializer.Deserialize<Project>(json, options);

            if (project == null)
            {
                throw new InvalidOperationException("Failed to deserialize project data from JSON");
            }

            await _cache.SetAsync(CACHE_KEY, project, DEFAULT_CACHE_TTL);
            _logger.LogInformation("Project data loaded successfully and cached");

            return project;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Data file not found at {Path}", DATA_FILE_PATH);
            throw new InvalidOperationException($"Configuration file not found at {DATA_FILE_PATH}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON from data file");
            throw new InvalidOperationException("Invalid JSON format in data file", ex);
        }
    }

    public void InvalidateCache()
    {
        _cache.Remove(CACHE_KEY);
        _logger.LogInformation("Project data cache invalidated");
    }
}