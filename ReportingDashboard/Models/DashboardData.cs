namespace ReportingDashboard.Models;

public class DashboardData
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string BacklogUrl { get; set; } = string.Empty;
    public List<string> Months { get; set; } = new();
    public List<MilestoneStream> MilestoneStreams { get; set; } = new();
    public string? NowDate { get; set; }
    public string TimelineStartDate { get; set; } = string.Empty;
    public string TimelineEndDate { get; set; } = string.Empty;
    public List<StatusCategory> Categories { get; set; } = new();
}