using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public interface IDataProvider
    {
        Task<ProjectMetrics> GetProjectMetricsAsync();
        Task<Project> GetProjectDataAsync();
    }
}