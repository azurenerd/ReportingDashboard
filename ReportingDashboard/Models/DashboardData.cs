using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();
}

public class ProjectInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("reportingPeriod")]
    public string ReportingPeriod { get; set; } = string.Empty;

    [JsonPropertyName("ragStatus")]
    public string RagStatus { get; set; } = "Green";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}