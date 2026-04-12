using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public interface IProjectDataService
    {
        Task InitializeAsync();
        Task<Project?> GetProjectAsync(Guid projectId);
        Task<Project?> GetCurrentProjectAsync();
        Task RefreshFromJsonAsync();
    }
}