using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDataProvider
{
    Task<Project> LoadProjectDataAsync();
    void InvalidateCache();
}