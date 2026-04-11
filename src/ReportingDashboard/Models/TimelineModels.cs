using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

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