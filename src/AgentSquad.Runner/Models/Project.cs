using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a project with milestones, work items, and health metrics.
/// </summary>
public class Project
{
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the project target end date.
    /// </summary>
    [JsonPropertyName("targetEndDate")]
    public DateTime TargetEndDate { get; set; }

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
    /// Gets or sets the velocity this month (items completed).
    /// </summary>
    [JsonPropertyName("velocityThisMonth")]
    public int VelocityThisMonth { get; set; }

    /// <summary>
    /// Gets or sets the list of project milestones.
    /// </summary>
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of project work items.
    /// </summary>
    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = [];
}