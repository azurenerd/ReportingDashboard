namespace ReportingDashboard.Web.Models;

public class HeatmapData
{
    public List<string> Months { get; set; } = new();
    public string CurrentMonth { get; set; } = "";
    public List<HeatmapCategory> Categories { get; set; } = new();
}