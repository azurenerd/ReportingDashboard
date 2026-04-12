using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(ILogger<ReportingService> logger)
        {
            _logger = logger;
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync(Guid projectId)
        {
            _logger.LogInformation("Getting dashboard metrics for project {ProjectId}", projectId);
            return await Task.FromResult(new DashboardMetrics
            {
                CompletionPercentage = 0,
                ShippedCount = 0,
                CarriedOverCount = 0,
                TotalWorkItems = 0,
                InProgressCount = 0,
                NewCount = 0
            });
        }

        public async Task<List<Milestone>> GetMilestonesAsync(Guid projectId)
        {
            _logger.LogInformation("Getting milestones for project {ProjectId}", projectId);
            return await Task.FromResult(new List<Milestone>());
        }

        public async Task<List<WorkItem>> GetWorkItemsAsync(Guid projectId, string? statusFilter = null, Guid? milestoneFilter = null)
        {
            _logger.LogInformation("Getting work items for project {ProjectId} with filter {StatusFilter}", projectId, statusFilter);
            return await Task.FromResult(new List<WorkItem>());
        }
    }
}