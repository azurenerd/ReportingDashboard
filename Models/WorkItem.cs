namespace AgentSquad.Runner.Models
{
    public class WorkItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public WorkItemStatus Status { get; set; }
        public string Category { get; set; }
        public string AssignedTo { get; set; }
    }
}