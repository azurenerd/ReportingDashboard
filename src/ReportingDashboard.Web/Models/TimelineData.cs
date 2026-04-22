namespace ReportingDashboard.Web.Models;

public class TimelineData
{
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public List<TimelineTrack> Tracks { get; set; } = new();
}
