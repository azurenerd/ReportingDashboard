using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("backlogLink")]
    public string BacklogLink { get; set; } = string.Empty;

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; set; } = string.Empty;

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; set; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; set; } = new();
}