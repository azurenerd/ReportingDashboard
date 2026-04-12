namespace AgentSquad.Runner.Models
{
    public class WorkItem
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? MilestoneId { get; set; }
        public string Title { get; set; } = string.Empty;
        public WorkItemStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? OwnerName { get; set; }

        public virtual Project? Project { get; set; }
        public virtual Milestone? Milestone { get; set; }
    }
}