using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = "";

    [JsonPropertyName("backlogLink")]
    public string BacklogLink { get; set; } = "";

    [JsonPropertyName("currentMonth")]
    public MonthSummary CurrentMonth { get; set; } = new();

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; set; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; set; } = new();

    [JsonPropertyName("project")]
    public ProjectInfo Project { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<MilestoneItem> Milestones { get; set; } = new();

    [JsonPropertyName("shipped")]
    public List<WorkItem> Shipped { get; set; } = new();

    [JsonPropertyName("inProgress")]
    public List<WorkItem> InProgress { get; set; } = new();

    [JsonPropertyName("carriedOver")]
    public List<WorkItem> CarriedOver { get; set; } = new();

    [JsonIgnore]
    public string ErrorMessage { get; set; } = "";
}

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("lead")]
    public string Lead { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";
}

public class MilestoneItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("targetDate")]
    public string TargetDate { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
}

public class MonthSummary
{
    [JsonPropertyName("month")]
    public string? Month { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("completedItems")]
    public int CompletedItems { get; set; }

    [JsonPropertyName("carriedItems")]
    public int CarriedItems { get; set; }

    [JsonPropertyName("overallHealth")]
    public string? OverallHealth { get; set; }
}