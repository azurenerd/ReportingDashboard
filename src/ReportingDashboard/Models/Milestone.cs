namespace ReportingDashboard.Models;

public class Milestone
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Position { get; set; }
}
