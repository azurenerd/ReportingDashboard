namespace AgentSquad.Runner.Services.Models;

/// <summary>
/// Root data model representing the complete project dashboard data.
/// Deserializes from data.json with Project object, Milestones array, and Tasks array.
/// </summary>
public class ProjectData
{
    public ProjectInfo? Project { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<ProjectTask> Tasks { get; set; } = new();

    /// <summary>
    /// Gets the project name from the nested Project object.
    /// </summary>
    public string ProjectName => Project?.Name ?? string.Empty;

    /// <summary>
    /// Gets the project start date from the nested Project object.
    /// </summary>
    public DateTime StartDate => Project?.StartDate ?? DateTime.MinValue;

    /// <summary>
    /// Gets the project end date from the nested Project object.
    /// </summary>
    public DateTime EndDate => Project?.EndDate ?? DateTime.MinValue;

    /// <summary>
    /// Gets the overall completion percentage from the nested Project object.
    /// </summary>
    public int OverallCompletionPercentage => Project?.OverallCompletionPercentage ?? 0;
}