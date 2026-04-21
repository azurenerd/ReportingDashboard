namespace ReportingDashboard.Web.Models;

public class HeatmapCategory
{
    public string Name { get; set; } = "";
    public string ColorClass { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
