namespace AgentSquad.Runner.Data;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Task status enumeration for project tasks.
/// </summary>
public enum TaskStatus
{
    /// <summary>Completed and delivered task.</summary>
    Shipped = 0,

    /// <summary>Task currently in progress.</summary>
    InProgress = 1,

    /// <summary>Task carried over from previous iteration or delayed.</summary>
    CarriedOver = 2
}

/// <summary>
/// Milestone status enumeration for project milestones.
/// </summary>
public enum MilestoneStatus
{
    /// <summary>Milestone has been completed.</summary>
    Completed = 0,

    /// <summary>Milestone is currently in progress.</summary>
    InProgress = 1,

    /// <summary>Milestone is pending and not yet started.</summary>
    Pending = 2
}

/// <summary>
/// Project information containing metadata and high-level details.
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Gets or sets the project name.
    /// Example: "Q2 Mobile App Release"
    /// </summary>
    [Required(ErrorMessage = "Project name is required")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// Example: "iOS and Android mobile app version 2.0 with new payment integration"
    /// </summary>
    [Required(ErrorMessage = "Project description is required")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    [Required(ErrorMessage = "Project start date is required")]
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the project end date (target completion).
    /// </summary>
    [Required(ErrorMessage = "Project end date is required")]
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the project status.
    /// Example: "OnTrack", "AtRisk", "Delayed"
    /// </summary>
    [Required(ErrorMessage = "Project status is required")]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the executive sponsor name.
    /// </summary>
    [Required(ErrorMessage = "Project sponsor is required")]
    [JsonPropertyName("sponsor")]
    public string Sponsor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project manager name.
    /// </summary>
    [Required(ErrorMessage = "Project manager is required")]
    [JsonPropertyName("projectManager")]
    public string ProjectManager { get; set; } = string.Empty;
}

/// <summary>
/// Milestone representing a significant project achievement or deliverable.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Gets or sets the unique milestone identifier.
    /// Example: "m1"
    /// </summary>
    [Required(ErrorMessage = "Milestone ID is required")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the milestone name.
    /// Example: "Design Review Complete"
    /// </summary>
    [Required(ErrorMessage = "Milestone name is required")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target completion date.
    /// </summary>
    [Required(ErrorMessage = "Target date is required")]
    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Gets or sets the actual completion date, or null if not yet completed.
    /// </summary>
    [JsonPropertyName("actualDate")]
    public DateTime? ActualDate { get; set; }

    /// <summary>
    /// Gets or sets the milestone status.
    /// </summary>
    [Required(ErrorMessage = "Milestone status is required")]
    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage (0-100).
    /// </summary>
    [Required(ErrorMessage = "Completion percentage is required")]
    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }
}