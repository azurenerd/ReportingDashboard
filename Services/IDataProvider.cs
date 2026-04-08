namespace AgentSquad.Runner.Services;

using AgentSquad.Runner.Models;

public interface IDataProvider
{
    Task<Project> LoadProjectDataAsync();
    void InvalidateCache();
}