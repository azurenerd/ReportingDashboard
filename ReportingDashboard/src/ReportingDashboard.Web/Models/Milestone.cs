namespace ReportingDashboard.Web.Models;

public class Milestone
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TargetDate { get; set; } = string.Empty;
    public string? CompletionDate { get; set; }
    public string Status { get; set; } = "Upcoming";
}