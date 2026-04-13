namespace ReportingDashboard.Web.Models;

public class WorkItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int MilestoneId { get; set; }

    public string Owner { get; set; } = string.Empty;

    public string Priority { get; set; } = "Medium";

    public string? Notes { get; set; }

    public string? StatusIndicator { get; set; }
}