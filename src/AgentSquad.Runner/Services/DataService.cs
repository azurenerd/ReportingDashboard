using System.Text.Json;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DataService : IDataService
{
    private readonly IWebHostEnvironment _env;

    public DataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<ProjectData> LoadProjectDataAsync()
    {
        var dataPath = Path.Combine(_env.WebRootPath, "data", "data.json");

        if (!File.Exists(dataPath))
        {
            throw new FileNotFoundException($"Data file not found: {dataPath}");
        }

        using var stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
        var projectData = await JsonSerializer.DeserializeAsync<ProjectData>(stream)
            ?? throw new InvalidOperationException("Failed to deserialize project data");

        return projectData;
    }
}