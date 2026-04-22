using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

/// <summary>
/// Root object for dashboard-data.json.
/// </summary>
public record DashboardData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; init; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; init; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; init; } = new();
}

/// <summary>
/// Project metadata displayed in the header.
/// </summary>
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