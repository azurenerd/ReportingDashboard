using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class DashboardData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = [];

    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = [];
}