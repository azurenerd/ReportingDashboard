using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class ProjectReport
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("reportDate")]
    public string ReportDate { get; set; } = string.Empty;

    [JsonPropertyName("executiveSummary")]
    public string ExecutiveSummary { get; set; } = string.Empty;

    [JsonPropertyName("overallStatus")]
    public OverallStatus OverallStatus { get; set; } = OverallStatus.OnTrack;

    [JsonPropertyName("backlogUrl")]
    public string BacklogUrl { get; set; } = string.Empty;

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();

    [JsonPropertyName("statusUpdate")]
    public StatusUpdate? StatusUpdate { get; set; }

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OverallStatus
{
    OnTrack,
    AtRisk,
    Blocked
}