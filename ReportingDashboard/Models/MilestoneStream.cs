namespace ReportingDashboard.Models;

public class MilestoneStream
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<TimelineEvent> Events { get; set; } = new();
}