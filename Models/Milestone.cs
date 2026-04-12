namespace AgentSquad.Runner.Models
{
    public class Milestone
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public MilestoneStatus Status { get; set; }
        public decimal CompletionPercentage { get; set; }

        public virtual Project? Project { get; set; }
        public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    }
}