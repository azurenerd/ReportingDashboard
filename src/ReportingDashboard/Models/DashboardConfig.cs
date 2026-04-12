using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardConfig
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; init; } = "";

    [JsonPropertyName("backlogLink")]
    public string BacklogLink { get; init; } = "";

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; init; } = "";

    [JsonPropertyName("months")]
    public List<string> Months { get; init; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; init; } = new();

    [JsonPropertyName("heatmapRows")]
    public List<HeatmapRow> HeatmapRows { get; init; } = new();
}

public record Milestone
{
    [JsonPropertyName("label")]
    public string Label { get; init; } = "";

    [JsonPropertyName("description")]
    public string Description { get; init; } = "";

    [JsonPropertyName("color")]
    public string Color { get; init; } = "#888";

    [JsonPropertyName("events")]
    public List<MilestoneEvent> Events { get; init; } = new();
}

public record MilestoneEvent
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = "";

    [JsonPropertyName("type")]
    public string Type { get; init; } = "checkpoint";

    [JsonPropertyName("label")]
    public string Label { get; init; } = "";
}

public record HeatmapRow
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = "";

    [JsonPropertyName("label")]
    public string Label { get; init; } = "";

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; init; } = new();
}