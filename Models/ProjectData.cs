using System.Text.Json.Serialization;

namespace AgentSquad.Dashboard.Models;

public class ProjectData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("tasks")]
    public List<ProjectTask> Tasks { get; set; } = new();

    [JsonPropertyName("metrics")]
    public ProjectMetrics Metrics { get; set; } = new();
}

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("sponsor")]
    public string Sponsor { get; set; } = string.Empty;

    [JsonPropertyName("projectManager")]
    public string ProjectManager { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("projectStartDate")]
    public DateTime ProjectStartDate { get; set; }

    [JsonPropertyName("projectEndDate")]
    public DateTime ProjectEndDate { get; set; }
}

public class Milestone
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }
}

public class ProjectTask
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TaskStatus Status { get; set; }

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
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

    [JsonPropertyName("totalCompletionPercentage")]
    public int TotalCompletionPercentage { get; set; }
}

public enum MilestoneStatus
{
    Completed,
    InProgress,
    Pending
}

public enum TaskStatus
{
    Shipped,
    InProgress,
    CarriedOver
}