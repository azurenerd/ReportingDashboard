using System.Text.Json.Serialization;

namespace AgentSquad.Dashboard.Models;

public class ProjectData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; set; } = null!;

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("tasks")]
    public List<Task> Tasks { get; set; } = new();

    [JsonPropertyName("metrics")]
    public ProjectMetrics Metrics { get; set; } = null!;
}

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; set; } = string.Empty;

    [JsonPropertyName("projectManager")]
    public string ProjectManager { get; set; } = string.Empty;
}

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    [JsonPropertyName("actualDate")]
    public DateTime? ActualDate { get; set; }

    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }
}

public enum MilestoneStatus
{
    Completed = 0,
    InProgress = 1,
    Pending = 2
}

public class Task
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TaskStatus Status { get; set; }

    [JsonPropertyName("assignedTo")]
    public string AssignedTo { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }

    [JsonPropertyName("estimatedDays")]
    public int EstimatedDays { get; set; }

    [JsonPropertyName("relatedMilestone")]
    public string RelatedMilestone { get; set; } = string.Empty;
}

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

    public int CompletionPercentage => TotalTasks > 0 
        ? (int)((CompletedTasks / (double)TotalTasks) * 100) 
        : 0;
}