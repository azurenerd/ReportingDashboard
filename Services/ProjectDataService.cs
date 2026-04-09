using System.Text.Json;
using AgentSquad.Dashboard.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Dashboard.Services;

public class ProjectDataService
{
    private readonly ILogger<ProjectDataService> _logger;
    private ProjectData? _cachedData;
    private DateTime _lastLoadTime;

    public ProjectDataService(ILogger<ProjectDataService> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"data.json not found at {jsonFilePath}");
            }

            var json = await File.ReadAllTextAsync(jsonFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<ProjectData>(json, options);

            if (data == null)
            {
                throw new InvalidOperationException("JSON deserialization resulted in null ProjectData");
            }

            _cachedData = data;
            _lastLoadTime = DateTime.UtcNow;
            _logger.LogInformation("Project data loaded successfully");
            return data;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in data.json");
            throw new InvalidOperationException($"Invalid JSON format: {ex.Message}", ex);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "data.json file not found");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading project data");
            throw new InvalidOperationException($"Error loading project data: {ex.Message}", ex);
        }
    }

    public ProjectData? GetCachedData() => _cachedData;

    public bool ValidateJsonSchema(string json)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<ProjectData>(json, options);
            return data != null && !string.IsNullOrEmpty(data.Project?.Name);
        }
        catch
        {
            return false;
        }
    }
}