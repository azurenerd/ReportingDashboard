using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class Project
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("targetEndDate")]
    public DateTime TargetEndDate { get; set; }

    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    [JsonPropertyName("healthStatus")]
    public HealthStatus HealthStatus { get; set; }

    [JsonPropertyName("velocityThisMonth")]
    public int VelocityThisMonth { get; set; }

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();
}

public enum HealthStatus
{
    OnTrack,
    AtRisk,
    Blocked
}