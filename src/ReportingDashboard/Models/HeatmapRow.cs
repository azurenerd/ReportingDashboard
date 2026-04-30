namespace ReportingDashboard.Models;

public class HeatmapRow
{
    public string Category { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
