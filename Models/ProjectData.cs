namespace AgentSquad.Runner.Models
{
    public class ProjectData
    {
        public ProjectInfo Project { get; set; }
        public List<Milestone> Milestones { get; set; } = new List<Milestone>();
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ProjectMetrics Metrics { get; set; }
    }

    public class ProjectInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Sponsor { get; set; }
        public string ProjectManager { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CompletionPercentage { get; set; }
    }

    public class ProjectMetrics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CarriedOverTasks { get; set; }
        public decimal EstimatedBurndownRate { get; set; }
    }

    public class Milestone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime? ActualDate { get; set; }
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
        public int EstimatedDays { get; set; }
        public string RelatedMilestone { get; set; }
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