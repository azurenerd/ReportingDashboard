using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDataProvider
{
    Task<ProjectData> LoadAsync(string filePath);
}