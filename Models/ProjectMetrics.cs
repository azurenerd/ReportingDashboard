using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class ProjectMetrics
{
    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    [JsonPropertyName("healthStatus")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HealthStatus HealthStatus { get; set; }

    [JsonPropertyName("velocityThisMonth")]
    public int VelocityThisMonth { get; set; }

    [JsonPropertyName("totalMilestones")]
    public int TotalMilestones { get; set; }

    [JsonPropertyName("completedMilestones")]
    public int CompletedMilestones { get; set; }
}