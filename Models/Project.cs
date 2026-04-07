using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents the complete project data including milestones, work items, and metrics.
/// </summary>
public class Project
{
    /// <summary>
    /// The name or title of the project.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the project's objectives and scope.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The project start date.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The target end date for the project.
    /// </summary>
    [JsonPropertyName("targetEndDate")]
    public DateTime TargetEndDate { get; set; }

    /// <summary>
    /// The overall project completion percentage (0-100).
    /// </summary>
    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// The current health status of the project.
    /// </summary>
    [JsonPropertyName("healthStatus")]
    public HealthStatus HealthStatus { get; set; }

    /// <summary>
    /// The number of work items completed this month.
    /// </summary>
    [JsonPropertyName("velocityThisMonth")]
    public int VelocityThisMonth { get; set; }

    /// <summary>
    /// The collection of milestones for the project.
    /// </summary>
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    /// <summary>
    /// The collection of work items tracked in the project.
    /// </summary>
    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();
}