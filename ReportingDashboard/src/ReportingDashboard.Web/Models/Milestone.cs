namespace ReportingDashboard.Web.Models;

public class Milestone
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string TargetDate { get; set; } = string.Empty;

    public string? CompletionDate { get; set; }

    public string Status { get; set; } = "Upcoming";

    public string Description { get; set; } = string.Empty;
}