using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class DashboardData
{
    [JsonPropertyName("projectInfo")]
    public ProjectInfo ProjectInfo { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();
}