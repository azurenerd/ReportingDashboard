namespace AgentSquad.Runner.Models;

public class WorkItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public WorkItemStatus Status { get; set; }
    public string AssignedTo { get; set; }
}

public enum WorkItemStatus
{
    Shipped,
    InProgress,
    CarriedOver
}

public static class WorkItemExtensions
{
    public static IEnumerable<IGrouping<WorkItemStatus, WorkItem>> GroupByStatus(this List<WorkItem> items)
    {
        var statusOrder = new[] { WorkItemStatus.Shipped, WorkItemStatus.InProgress, WorkItemStatus.CarriedOver };
        return items
            .GroupBy(i => i.Status)
            .OrderBy(g => System.Array.IndexOf(statusOrder, g.Key));
    }
}