namespace ReportingDashboard.Models;

public class StatusCategory
{
    public string Name { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Items { get; set; } = new();
}