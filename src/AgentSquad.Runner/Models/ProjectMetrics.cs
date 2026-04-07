using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents key performance indicators and health metrics for a project.
/// </summary>
public class ProjectMetrics
{
    /// <summary>
    /// Gets or sets the overall project completion percentage (0-100).
    /// </summary>
    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the project health status indicator.
    /// </summary>
    [JsonPropertyName("healthStatus")]
    public HealthStatus HealthStatus { get; set; }

    /// <summary>
    /// Gets or sets the count of work items completed this month (velocity indicator).
    /// </summary>
    [JsonPropertyName("velocityThisMonth")]
    public int VelocityThisMonth { get; set; }

    /// <summary>
    /// Gets or sets the total number of milestones in the project.
    /// </summary>
    [JsonPropertyName("totalMilestones")]
    public int TotalMilestones { get; set; }

    /// <summary>
    /// Gets or sets the number of milestones that have been completed.
    /// </summary>
    [JsonPropertyName("completedMilestones")]
    public int CompletedMilestones { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectMetrics"/> class.
    /// </summary>
    public ProjectMetrics()
    {
    }
}