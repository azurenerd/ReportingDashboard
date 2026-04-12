using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public interface IReportingService
    {
        Task<DashboardMetrics> GetDashboardMetricsAsync(Guid projectId);
        Task<List<Milestone>> GetMilestonesAsync(Guid projectId);
        Task<List<WorkItem>> GetWorkItemsAsync(Guid projectId, string? statusFilter = null, Guid? milestoneFilter = null);
    }
}