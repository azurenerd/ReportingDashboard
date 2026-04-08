namespace AgentSquad.Runner.Models
{
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Milestone> Milestones { get; set; } = new();
        public List<WorkItem> WorkItems { get; set; } = new();
    }

    public class Milestone
    {
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public MilestoneStatus Status { get; set; }
        public string Description { get; set; }
    }

    public class WorkItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public WorkItemStatus Status { get; set; }
    }
}