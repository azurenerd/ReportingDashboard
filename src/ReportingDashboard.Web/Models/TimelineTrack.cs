namespace ReportingDashboard.Web.Models;

public class TimelineTrack
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Color { get; set; } = "";
    public List<Milestone> Milestones { get; set; } = new();
}
