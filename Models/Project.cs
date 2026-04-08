using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Root project model containing all dashboard data.
/// </summary>
public class Project
{
    /// <summary>
    /// Project name (required).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Project description (optional).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Project start date.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Project target end date.
    /// </summary>
    [JsonPropertyName("targetEndDate")]
    public DateTime TargetEndDate { get; set; }

    /// <summary>
    /// Overall project completion percentage (0-100).
    /// </summary>
    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Current project health status.
    /// </summary>
    [JsonPropertyName("healthStatus")]
    public HealthStatus HealthStatus { get; set; }

    /// <summary>
    /// Number of work items completed this month.
    /// </summary>
    [JsonPropertyName("velocityThisMonth")]
    public int VelocityThisMonth { get; set; }

    /// <summary>
    /// Collection of project milestones (required, at least 1).
    /// </summary>
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    /// <summary>
    /// Collection of work items tracked in dashboard.
    /// </summary>
    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();
}