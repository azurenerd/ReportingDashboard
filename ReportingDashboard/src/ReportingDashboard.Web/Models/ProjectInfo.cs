using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("executiveSponsor")]
    public string ExecutiveSponsor { get; set; } = string.Empty;

    [JsonPropertyName("reportingPeriod")]
    public string ReportingPeriod { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProjectStatus Status { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectStatus
{
    OnTrack,
    AtRisk,
    Blocked
}