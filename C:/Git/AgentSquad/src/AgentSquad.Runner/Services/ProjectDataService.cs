using System.Text.Json;
using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Services;

public class ProjectDataService
{
    private ProjectData _cachedData;
    private DateTime _lastLoadTime;
    private readonly ILogger<ProjectDataService> _logger;

    public ProjectDataService(ILogger<ProjectDataService> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectData> LoadProjectDataAsync(string relativePath)
    {
        try
        {
            var wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var fullPath = Path.Combine(wwwrootPath, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"data.json not found at {fullPath}");
            }

            var json = await File.ReadAllTextAsync(fullPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                throw new InvalidOperationException("JSON deserialization resulted in null");
            }

            if (data.Project == null || data.Milestones == null || data.Tasks == null)
            {
                throw new InvalidOperationException("Missing required fields in JSON: Project, Milestones, or Tasks");
            }

            _cachedData = data;
            _lastLoadTime = DateTime.UtcNow;
            _logger.LogInformation("Project data loaded successfully at {Time}", _lastLoadTime);
            return data;
        }
        catch (JsonException ex)
        {
            var msg = $"Invalid JSON format: {ex.Message}";
            _logger.LogError(msg);
            throw new InvalidOperationException(msg, ex);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error loading project data: {Message}", ex.Message);
            throw;
        }
    }

    public ProjectData GetCachedData()
    {
        return _cachedData;
    }

    public void RefreshData()
    {
        _cachedData = null;
        _lastLoadTime = DateTime.MinValue;
    }

    public bool ValidateJsonSchema(string json)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<ProjectData>(json, options);
            return data?.Project != null && data.Milestones != null && data.Tasks != null;
        }
        catch
        {
            return false;
        }
    }
}