#nullable enable

using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Root configuration object for the dashboard, loaded from data.json.
/// Represents the complete project status data including quarters and milestones.
/// </summary>
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