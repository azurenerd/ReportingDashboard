using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class DashboardConfig
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quarters")]
    public List<Quarter> Quarters { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}