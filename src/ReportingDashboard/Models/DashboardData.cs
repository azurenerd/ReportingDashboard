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
    public string Title { get; init; } = "";

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; init; } = "";

    [JsonPropertyName("backlogUrl")]
    public string? BacklogUrl { get; init; }

    [JsonPropertyName("currentDate")]
    public string CurrentDate { get; init; } = "";
}