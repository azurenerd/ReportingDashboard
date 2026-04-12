using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class ProjectMetrics
{
    [JsonPropertyName("totalStoryPoints")]
    public int TotalStoryPoints { get; set; }

    [JsonPropertyName("completedStoryPoints")]
    public int CompletedStoryPoints { get; set; }

    [JsonPropertyName("inProgressStoryPoints")]
    public int InProgressStoryPoints { get; set; }

    [JsonPropertyName("carriedOverCount")]
    public int CarriedOverCount { get; set; }

    [JsonPropertyName("velocityPerSprint")]
    public double VelocityPerSprint { get; set; }
}