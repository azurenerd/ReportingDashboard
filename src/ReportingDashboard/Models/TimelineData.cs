using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

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
    public List<TimelineMilestone> Milestones { get; set; } = new();
}

public class TimelineMilestone
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkpoint";

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}