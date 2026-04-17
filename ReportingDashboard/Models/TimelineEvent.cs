namespace ReportingDashboard.Models;

public class TimelineEvent
{
    public string Date { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? LabelPosition { get; set; }
}