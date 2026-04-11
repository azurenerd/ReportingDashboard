namespace ReportingDashboard.Models;

public class HeatmapData
{
    public Dictionary<string, List<string>> Shipped { get; set; } = new();
    public Dictionary<string, List<string>> InProgress { get; set; } = new();
    public Dictionary<string, List<string>> Carryover { get; set; } = new();
    public Dictionary<string, List<string>> Blockers { get; set; } = new();
}