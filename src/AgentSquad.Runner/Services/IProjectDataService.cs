using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IProjectDataService
{
    Task InitializeAsync();
    ProjectDashboard GetDashboard();
    Task RefreshAsync();
    bool IsInitialized { get; }
    string? LastError { get; }
}