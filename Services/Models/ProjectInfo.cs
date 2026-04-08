namespace AgentSquad.Runner.Services.Models;

/// <summary>
/// Contains core project metadata (name, dates, completion percentage).
/// </summary>
public class ProjectInfo
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int OverallCompletionPercentage { get; set; }
}