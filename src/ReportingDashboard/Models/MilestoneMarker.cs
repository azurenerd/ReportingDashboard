using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class MilestoneMarker
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}