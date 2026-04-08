using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    public class ProjectData
    {
        [JsonPropertyName("project")]
        public ProjectInfo Project { get; set; }

        [JsonPropertyName("milestones")]
        public List<Milestone> Milestones { get; set; } = new List<Milestone>();

        [JsonPropertyName("tasks")]
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        [JsonPropertyName("metrics")]
        public ProjectMetrics Metrics { get; set; }
    }

    public class ProjectInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("sponsor")]
        public string Sponsor { get; set; }

        [JsonPropertyName("projectManager")]
        public string ProjectManager { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }
    }

    public class ProjectMetrics
    {
        [JsonPropertyName("totalTasks")]
        public int TotalTasks { get; set; }

        [JsonPropertyName("completedTasks")]
        public int CompletedTasks { get; set; }

        [JsonPropertyName("inProgressTasks")]
        public int InProgressTasks { get; set; }

        [JsonPropertyName("carriedOverTasks")]
        public int CarriedOverTasks { get; set; }

        [JsonPropertyName("estimatedBurndownRate")]
        public decimal EstimatedBurndownRate { get; set; }
    }

    public class Milestone
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("targetDate")]
        public DateTime TargetDate { get; set; }

        [JsonPropertyName("actualDate")]
        public DateTime? ActualDate { get; set; }

        [JsonPropertyName("status")]
        public MilestoneStatus Status { get; set; }

        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }
    }

    public class ProjectTask
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public TaskStatus Status { get; set; }

        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonPropertyName("estimatedDays")]
        public int EstimatedDays { get; set; }

        [JsonPropertyName("relatedMilestone")]
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