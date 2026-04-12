using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public interface IMilestoneService
    {
        Task<List<Milestone>> GetMilestonesByDateRangeAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetMilestoneCompletionPercentageAsync(Guid milestoneId);
        Task<List<WorkItem>> GetWorkItemsByMilestoneAsync(Guid milestoneId);
    }
}