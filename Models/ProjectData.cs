namespace AgentSquad.Runner.Models
{
    public class ProjectData
    {
        public string ProjectName { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
        public int CompletionPercentage { get; set; }
        public List<Milestone> Milestones { get; set; } = new List<Milestone>();
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }

    public class Milestone
    {
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public MilestoneStatus Status { get; set; }
        public int CompletionPercentage { get; set; }
    }

    public class ProjectTask
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TaskStatus Status { get; set; }
        public string AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
    }

    public enum MilestoneStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public enum TaskStatus
    {
        Shipped,
        InProgress,
        CarriedOver
    }
}