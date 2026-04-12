using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class ProjectMetadata
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // "On Track", "At Risk", "Off Track"

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("targetEndDate")]
    public DateTime TargetEndDate { get; set; }
}