namespace ReportingDashboard.Models;

public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BacklogUrl { get; set; } = "";
    public DateTime CurrentDate { get; set; }
    public DateTime TimelineStartDate { get; set; }
    public DateTime TimelineEndDate { get; set; }
    public List<MilestoneStream> MilestoneStreams { get; set; } = new();
    public List<string> HeatmapMonths { get; set; } = new();
    public string CurrentMonth { get; set; } = "";
    public List<HeatmapRow> HeatmapRows { get; set; } = new();
}
