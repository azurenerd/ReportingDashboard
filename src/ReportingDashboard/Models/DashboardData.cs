namespace ReportingDashboard.Models;

public class DashboardData
{
    public string Title { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Workstream { get; set; } = string.Empty;
    public string CurrentMonth { get; set; } = string.Empty;
    public string BacklogUrl { get; set; } = string.Empty;
    public DateTime? NowDateOverride { get; set; }
    public List<string> Months { get; set; } = new();
    public int CurrentMonthIndex { get; set; }
    public TimelineConfig Timeline { get; set; } = new();
    public HeatmapData Heatmap { get; set; } = new();
}