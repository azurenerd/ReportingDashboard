using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDashboardDataService
{
    Task<DashboardConfig> GetDashboardConfigAsync();
    Task RefreshAsync();
    DateTime GetLastModifiedTime();
}