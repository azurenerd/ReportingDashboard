using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class ProjectData
{
    [JsonPropertyName("project")]
    public ProjectMetadata Project { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();

    [JsonPropertyName("metrics")]
    public ProjectMetrics Metrics { get; set; } = new();
}