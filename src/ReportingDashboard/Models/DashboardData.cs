using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("backlogUrl")]
    public string BacklogUrl { get; set; } = string.Empty;

    [JsonPropertyName("currentDate")]
    public DateTime CurrentDate { get; set; }

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("currentMonthIndex")]
    public int CurrentMonthIndex { get; set; }

    [JsonPropertyName("timelineStart")]
    public DateTime TimelineStart { get; set; }

    [JsonPropertyName("timelineEnd")]
    public DateTime TimelineEnd { get; set; }

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<HeatmapCategory> Categories { get; set; } = new();
}