using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("nowDate")]
    public string NowDate { get; set; } = string.Empty;

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new();
}