namespace AgentSquad.Runner.Models;

public class ReportData
{
    public string ReportTitle { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string AdoBacklogUrl { get; set; } = string.Empty;
    public List<Milestone> Milestones { get; set; } = new();
    public List<StatusRow> StatusRows { get; set; } = new();
}