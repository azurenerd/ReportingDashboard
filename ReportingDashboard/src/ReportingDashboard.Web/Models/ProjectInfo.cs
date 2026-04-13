using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class ProjectInfo
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("executiveSponsor")]
    public string ExecutiveSponsor { get; set; } = string.Empty;

    [JsonPropertyName("reportingPeriod")]
    public string ReportingPeriod { get; set; } = string.Empty;

    [JsonPropertyName("overallStatus")]
    public string OverallStatus { get; set; } = "OnTrack";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}