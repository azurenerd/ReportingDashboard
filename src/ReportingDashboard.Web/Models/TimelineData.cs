using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("nowDate")]
    public string NowDate { get; set; } = "";

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new();
}