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

    public async Task<ProjectData> LoadProjectDataAsync()
    {
        try
        {
            var dataPath = Path.Combine(_environment.WebRootPath, "data.json");

            if (!File.Exists(dataPath))
            {
                return new ProjectData { Project = new ProjectInfo() };
            }

            var json = await File.ReadAllTextAsync(dataPath);
            var data = JsonSerializer.Deserialize<ProjectData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data != null)
            {
                ValidateJsonSchema(data);
                _cachedData = data;
                _lastLoadTime = DateTime.UtcNow;
            }

            return data ?? new ProjectData { Project = new ProjectInfo() };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading project data: {ex.Message}");
            return new ProjectData { Project = new ProjectInfo() };
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

    public async Task<ProjectData> RefreshData()
    {
        _cachedData = null;
        _lastLoadTime = DateTime.MinValue;
        return await LoadProjectDataAsync();
    }

    public bool ValidateJsonSchema(ProjectData data)
    {
        if (data?.Project == null)
            throw new InvalidOperationException("Project information is required in data.json");

        if (string.IsNullOrWhiteSpace(data.Project.Name))
            throw new InvalidOperationException("Project name is required");

        foreach (var milestone in data.Milestones ?? new())
        {
            if (string.IsNullOrWhiteSpace(milestone.Name))
                throw new InvalidOperationException("Milestone name is required");

            if (milestone.TargetDate == default)
                throw new InvalidOperationException("Milestone target date is required");
        }

        foreach (var task in data.Tasks ?? new())
        {
            if (string.IsNullOrWhiteSpace(task.Name))
                throw new InvalidOperationException("Task name is required");

            if (string.IsNullOrWhiteSpace(task.Owner))
                throw new InvalidOperationException("Task owner is required");
        }

        return true;
    }
}