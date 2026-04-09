using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AgentSquad.Dashboard.Data;

public class ProjectData
{
    [JsonPropertyName("project")]
    public ProjectInfo? Project { get; set; }

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("tasks")]
    public List<Task> Tasks { get; set; } = new();

    [JsonPropertyName("metrics")]
    public ProjectMetrics? Metrics { get; set; }
}

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "OnTrack";

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; set; } = "";

    [JsonPropertyName("projectManager")]
    public string ProjectManager { get; set; } = "";
}

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    [JsonPropertyName("actualDate")]
    public DateTime? ActualDate { get; set; }

    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneStatus
{
    Completed = 0,
    InProgress = 1,
    Pending = 2
}

public class Task
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("status")]
    public TaskStatus Status { get; set; }

    [JsonPropertyName("assignedTo")]
    public string AssignedTo { get; set; } = "";

    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }

    [JsonPropertyName("estimatedDays")]
    public int EstimatedDays { get; set; }

    [JsonPropertyName("relatedMilestone")]
    public string RelatedMilestone { get; set; } = "";
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
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
    public double EstimatedBurndownRate { get; set; }

    public int CompletionPercentage => TotalTasks > 0 ? (CompletedTasks * 100) / TotalTasks : 0;
}