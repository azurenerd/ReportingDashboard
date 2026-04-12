namespace AgentSquad.Runner.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime TargetEndDate { get; set; }

        public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
        public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    }
}