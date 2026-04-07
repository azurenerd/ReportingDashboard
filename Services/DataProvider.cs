using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DATA_FILE_PATH = "wwwroot/data.json";
    private const string CACHE_KEY = "project_data";

    public DataProvider(IDataCache cache, ILogger<DataProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<Project> LoadProjectDataAsync()
    {
        var cached = await _cache.GetAsync<Project>(CACHE_KEY);
        if (cached != null) return cached;

        try
        {
            if (!File.Exists(DATA_FILE_PATH))
                throw new FileNotFoundException($"Configuration file not found: {DATA_FILE_PATH}");

            var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
            var project = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Failed to deserialize project data");

            ValidateProjectData(project);

            await _cache.SetAsync(CACHE_KEY, project, TimeSpan.FromHours(1));
            return project;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Data file not found");
            throw new InvalidOperationException("Configuration file missing. Please ensure wwwroot/data.json exists.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format");
            throw new InvalidOperationException("Invalid JSON format in data.json. Please check file syntax.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading project data");
            throw;
        }
    }

    public void InvalidateCache() => _cache.Remove(CACHE_KEY);

    private void ValidateProjectData(Project project)
    {
        if (project == null)
            throw new InvalidOperationException("Project data is null");
        if (string.IsNullOrWhiteSpace(project.Name))
            throw new InvalidOperationException("Project name is required");
        if (project.Milestones == null || project.Milestones.Count == 0)
            throw new InvalidOperationException("At least one milestone is required");
    }
}