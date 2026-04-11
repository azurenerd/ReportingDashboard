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
    public string CurrentMonth { get; set; } = "";

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
    public string StartDate { get; set; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("nowDate")]
    public string NowDate { get; set; } = "";

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new();
}

public class TimelineTrack
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#0078D4";

    [JsonPropertyName("milestones")]
    public List<MilestoneMarker> Milestones { get; set; } = new();
}

public class MilestoneMarker
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkpoint";
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