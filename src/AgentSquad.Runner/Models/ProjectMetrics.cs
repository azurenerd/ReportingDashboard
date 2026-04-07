using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents aggregated project metrics for dashboard display.
/// </summary>
public class ProjectMetrics
{
    /// <summary>
    /// Initializes a new instance of the ProjectMetrics class.
    /// </summary>
    public ProjectMetrics() { }

    /// <summary>
    /// Gets or sets the overall completion percentage (0-100).
    /// </summary>
    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the project health status.
    /// </summary>
    [JsonPropertyName("healthStatus")]
    public HealthStatus HealthStatus { get; set; }

    /// <summary>
    /// Gets or sets the number of items completed this month.
    /// </summary>
    [JsonPropertyName("velocityThisMonth")]
    public int VelocityThisMonth { get; set; }

    /// <summary>
    /// Gets or sets the total number of project milestones.
    /// </summary>
    [JsonPropertyName("totalMilestones")]
    public int TotalMilestones { get; set; }

    /// <summary>
    /// Gets or sets the number of completed milestones.
    /// </summary>
    [JsonPropertyName("completedMilestones")]
    public int CompletedMilestones { get; set; }
}