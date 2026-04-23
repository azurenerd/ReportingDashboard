using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; init; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; init; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; init; } = new();
}

public record ProjectInfo
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; init; } = string.Empty;

    [JsonPropertyName("backlogUrl")]
    public string? BacklogUrl { get; init; }

    [JsonPropertyName("currentDate")]
    public string CurrentDate { get; init; } = string.Empty;
}

public record TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = string.Empty;

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; init; } = new();
}

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

public record MilestoneItem
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = "checkpoint";
}

public record HeatmapData
{
    [JsonPropertyName("months")]
    public List<string> Months { get; init; } = new();

    [JsonPropertyName("highlightMonth")]
    public string HighlightMonth { get; init; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<StatusRow> Rows { get; init; } = new();
}

public record StatusRow
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; init; } = new();
}