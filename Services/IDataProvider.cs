using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDataProvider
{
    Project GetProjectData();
    bool IsLoaded { get; }
    string ErrorMessage { get; }
}