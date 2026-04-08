using AgentSquad.Runner.Data;
using System.Text.Json;

namespace AgentSquad.Runner.Services;

public class ProjectDataService
{
    private readonly IWebHostEnvironment _environment;
    private ProjectData? _cachedData;
    private DateTime _lastLoadTime = DateTime.MinValue;
    private const int CacheDurationSeconds = 300;

    public ProjectDataService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new DataLoadException($"Data file not found at path: {jsonFilePath}");
            }

            var json = await File.ReadAllTextAsync(jsonFilePath);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataLoadException("Data file is empty");
            }

            ValidateJsonSchema(json);

            var data = JsonSerializer.Deserialize<ProjectData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null)
            {
                throw new DataLoadException("Failed to deserialize project data");
            }

            _cachedData = data;
            _lastLoadTime = DateTime.UtcNow;

            return data;
        }
        catch (DataLoadException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new DataLoadException($"Invalid JSON format in data file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new DataLoadException($"Error loading project data: {ex.Message}", ex);
        }
    }

    public ProjectData? GetCachedData()
    {
        if (_cachedData != null && (DateTime.UtcNow - _lastLoadTime).TotalSeconds < CacheDurationSeconds)
        {
            return _cachedData;
        }

        return null;
    }

    public async Task<ProjectData> RefreshData(string jsonFilePath)
    {
        _cachedData = null;
        _lastLoadTime = DateTime.MinValue;
        return await LoadProjectDataAsync(jsonFilePath);
    }

    public bool ValidateJsonSchema(string json)
    {
        try
        {
            var data = JsonSerializer.Deserialize<ProjectData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.Project == null)
                throw new DataLoadException("Project information is required in data.json");

            if (string.IsNullOrWhiteSpace(data.Project.Name))
                throw new DataLoadException("Project name is required");

            foreach (var milestone in data.Milestones ?? new())
            {
                if (string.IsNullOrWhiteSpace(milestone.Name))
                    throw new DataLoadException("Milestone name is required");

                if (milestone.TargetDate == default)
                    throw new DataLoadException("Milestone target date is required");
            }

            foreach (var task in data.Tasks ?? new())
            {
                if (string.IsNullOrWhiteSpace(task.Name))
                    throw new DataLoadException("Task name is required");

                if (string.IsNullOrWhiteSpace(task.Owner))
                    throw new DataLoadException("Task owner is required");
            }

            return true;
        }
        catch (DataLoadException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new DataLoadException($"JSON validation failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new DataLoadException($"Schema validation error: {ex.Message}", ex);
        }
    }
}