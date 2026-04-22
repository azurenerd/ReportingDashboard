using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = "";

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; init; } = new();
}

public record TimelineTrack
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("color")]
    public string Color { get; init; } = "#999";

    [JsonPropertyName("milestones")]
    public List<MilestoneItem> Milestones { get; init; } = new();
}

public record MilestoneItem
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = "";

    [JsonPropertyName("label")]
    public string Label { get; init; } = "";

    [JsonPropertyName("type")]
    public string Type { get; init; } = "checkpoint";
}