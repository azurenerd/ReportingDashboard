namespace AgentSquad.Runner.Models
{
    public class WorkItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public WorkItemStatus Status { get; set; }
    }

    public enum WorkItemStatus
    {
        ShippedThisMonth,
        InProgress,
        CarriedOver
    }
}