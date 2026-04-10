using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDashboardDataService
{
    event Action OnDataChanged;

    DashboardData GetCurrentData();
    Project GetProject();
    IReadOnlyList<Milestone> GetMilestones();
    IReadOnlyList<WorkItem> GetWorkItems();
    IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status);
    (int Shipped, int InProgress, int CarriedOver) GetStatusCounts();
    string GetLastError();
    bool HasData { get; }
}