using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    public class Project
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("targetEndDate")]
        public DateTime TargetEndDate { get; set; }

        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }

        [JsonPropertyName("healthStatus")]
        public HealthStatus HealthStatus { get; set; }

        [JsonPropertyName("velocityThisMonth")]
        public int VelocityThisMonth { get; set; }

        [JsonPropertyName("milestones")]
        public List<Milestone> Milestones { get; set; } = new();

        [JsonPropertyName("workItems")]
        public List<WorkItem> WorkItems { get; set; } = new();
    }

    public class Milestone
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("targetDate")]
        public DateTime TargetDate { get; set; }

        [JsonPropertyName("status")]
        public MilestoneStatus Status { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class WorkItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("status")]
        public WorkItemStatus Status { get; set; }

        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MilestoneStatus
    {
        Completed,
        InProgress,
        AtRisk,
        Future
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkItemStatus
    {
        Shipped,
        InProgress,
        CarriedOver
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HealthStatus
    {
        OnTrack,
        AtRisk,
        Blocked
    }
}