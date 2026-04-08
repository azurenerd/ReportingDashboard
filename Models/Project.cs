namespace AgentSquad.Runner.Models
{
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<Milestone> Milestones { get; set; } = new();
        public ProjectMetrics Metrics { get; set; }
        public List<WorkItem> WorkItems { get; set; } = new();
    }
}