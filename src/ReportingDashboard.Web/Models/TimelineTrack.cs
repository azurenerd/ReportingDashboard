using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class TimelineTrack
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#0078D4";

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}