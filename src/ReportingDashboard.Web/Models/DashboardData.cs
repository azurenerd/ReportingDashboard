namespace ReportingDashboard.Web.Models;

public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BacklogUrl { get; set; } = "";
    public string CurrentDate { get; set; } = "";
    public TimelineData Timeline { get; set; } = new();
    public HeatmapData Heatmap { get; set; } = new();
}