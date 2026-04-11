using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("backlogLink")]
    public string BacklogLink { get; set; } = string.Empty;

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; set; } = string.Empty;

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; set; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; set; } = new();
}

public class TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("nowDate")]
    public string NowDate { get; set; } = string.Empty;

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new();
}

public class TimelineTrack
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#999";

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkpoint";

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}

public class HeatmapData
{
    [JsonPropertyName("shipped")]
    public Dictionary<string, List<string>> Shipped { get; set; } = new();

    [JsonPropertyName("inProgress")]
    public Dictionary<string, List<string>> InProgress { get; set; } = new();

    [JsonPropertyName("carryover")]
    public Dictionary<string, List<string>> Carryover { get; set; } = new();

    [JsonPropertyName("blockers")]
    public Dictionary<string, List<string>> Blockers { get; set; } = new();
}