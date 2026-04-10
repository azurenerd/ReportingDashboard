using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDataService
{
    Task<ProjectStatus> ReadProjectDataAsync();
}