using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("nowDate")]
    public string NowDate { get; set; } = "";

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new List<TimelineTrack>();
}

public class TimelineTrack
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#999";

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new List<Milestone>();
}

public class Milestone
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkpoint";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";
}