using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

/// <summary>
/// Timeline section: date range and project tracks with milestones.
/// </summary>
public record TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = string.Empty;

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; init; } = new();
}

/// <summary>
/// A single horizontal track in the timeline (e.g., M1, M2).
/// </summary>
public record TimelineTrack
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; init; } = "#999";

    [JsonPropertyName("milestones")]
    public List<MilestoneItem> Milestones { get; init; } = new();
}

/// <summary>
/// A single milestone on a timeline track.
/// Type must be one of: "checkpoint", "poc", "production".
/// </summary>
public record MilestoneItem
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = "checkpoint";
}