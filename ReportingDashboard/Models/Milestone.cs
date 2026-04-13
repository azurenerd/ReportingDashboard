namespace ReportingDashboard.Models;

public class Milestone
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Status { get; set; } = string.Empty; // completed, in-progress, upcoming
    public string Color { get; set; } = string.Empty;
    public string MarkerType { get; set; } = string.Empty;
}