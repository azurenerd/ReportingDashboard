namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents the complete project data model including metadata and task list.
/// </summary>
public class ProjectData
{
    /// <summary>
    /// Project name or title.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed project description.
    /// </summary>
    public string ProjectDescription { get; set; } = string.Empty;

    /// <summary>
    /// Project start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Project end date or planned completion date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Overall project completion percentage (0-100).
    /// </summary>
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// List of all project tasks.
    /// </summary>
    public List<TaskItem> Tasks { get; set; } = new();

    /// <summary>
    /// List of project milestones.
    /// </summary>
    public List<Milestone> Milestones { get; set; } = new();
}