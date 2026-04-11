using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; init; } = string.Empty;

    [JsonPropertyName("backlogLink")]
    public string BacklogLink { get; init; } = string.Empty;

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; init; } = string.Empty;

    [JsonPropertyName("months")]
    public List<string> Months { get; init; } = [];

    [JsonPropertyName("project")]
    public ProjectInfo? Project { get; init; }

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; init; } = [];

    [JsonPropertyName("shipped")]
    public List<WorkItem> Shipped { get; init; } = [];

    [JsonPropertyName("inProgress")]
    public List<WorkItem> InProgress { get; init; } = [];

    [JsonPropertyName("carriedOver")]
    public List<WorkItem> CarriedOver { get; init; } = [];

    [JsonPropertyName("currentMonthSummary")]
    public MonthSummary? CurrentMonthSummary { get; init; }

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; init; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; init; } = new();

    public string? ErrorMessage { get; init; }
}

public record ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "Untitled Project";

    [JsonPropertyName("lead")]
    public string? Lead { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Unknown";

    [JsonPropertyName("lastUpdated")]
    public string? LastUpdated { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
}

public record Milestone
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("targetDate")]
    public string? TargetDate { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Upcoming";
}

public record WorkItem
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("percentComplete")]
    public int PercentComplete { get; init; }

    [JsonPropertyName("carryOverReason")]
    public string? CarryOverReason { get; init; }
}

public record MonthSummary
{
    [JsonPropertyName("month")]
    public string? Month { get; init; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; init; }

    [JsonPropertyName("completedItems")]
    public int CompletedItems { get; init; }

    [JsonPropertyName("carriedItems")]
    public int CarriedItems { get; init; }

    [JsonPropertyName("overallHealth")]
    public string OverallHealth { get; init; } = "Unknown";
}

public record TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = string.Empty;

    [JsonPropertyName("nowDate")]
    public string NowDate { get; init; } = string.Empty;

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; init; } = [];
}

public record TimelineTrack
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; init; } = "#999";

    [JsonPropertyName("milestones")]
    public List<TimelineMilestone> Milestones { get; init; } = [];
}

public record TimelineMilestone
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = "checkpoint";

    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;
}

public record HeatmapData
{
    [JsonPropertyName("shipped")]
    public Dictionary<string, List<string>> Shipped { get; init; } = new();

    [JsonPropertyName("inProgress")]
    public Dictionary<string, List<string>> InProgressItems { get; init; } = new();

    [JsonPropertyName("carryover")]
    public Dictionary<string, List<string>> Carryover { get; init; } = new();

    [JsonPropertyName("blockers")]
    public Dictionary<string, List<string>> Blockers { get; init; } = new();
}